#region using
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
#if Debug_Pesticides
using EFSAMessageCreator_Pesticides;
#elif Debug_Chemicals
using EFSAMessageCreator_Chemicals;
#elif Debug_Veterinary
using EFSAMessageCreator_Veterinary;
#elif Release_Pesticides
using EFSAMessageCreator_Pesticides;
#elif Release_Chemicals
using EFSAMessageCreator_Chemicals;
#elif Release_Veterinary
using EFSAMessageCreator_Veterinary;
#endif

using Microsoft.VisualBasic.FileIO;
#endregion

namespace EFSAMessageCreator
{
    /// <summary>
    /// The Class used for XML Generation
    /// </summary>
    class XMLGeneration
    {
#region Variables
        protected MainWindow mainWindowObject;
        protected String xsd;
        protected String elementMapping;
        protected string outputXMLFileName;
        protected Int32 validationErrorCount = 0;
#endregion

#region Constructor
        /// <summary>
        /// XML Generation
        /// </summary>
        /// <param name="MainWindowObject">The Object of the Application's Main Window</param>
        /// <param name="XSDFileFullName">The Full Name of the embedded XSD File</param>
        /// <param name="ElementMappingFileFullName">The Full Name of the embedded Element Mapping File</param>
        /// <param name="OutputXMLFileName">The Name of the Output XML File</param>
        /// <param name="OutputXMLEncoding">The Encoding (UTF-8 or Unicode UTF-16) of the Output XML File</param>
        public XMLGeneration(
            MainWindow MainWindowObject
            , String XSDFileFullName
            , String ElementMappingFileFullName
            , String OutputXMLFileName
            , System.Text.Encoding OutputXMLEncoding)
        {
            try
            {
                mainWindowObject = MainWindowObject;
                outputXMLFileName = OutputXMLFileName;
                xsd = XSDFileFullName;
                elementMapping = ElementMappingFileFullName;

                FileInfo fiDataFile = new FileInfo(mainWindowObject.tbxDataFile.Text);

                String[] csvExtensions = { ".csv", ".txt" };

                // Get the Data Rows
                List<DataRow> rows;
                if (fiDataFile.Extension.ToLower() == ".dbf")
                {
                    rows = GetDataRows();
                    SerializeRows(rows, OutputXMLEncoding);
                }
                else if (csvExtensions.Contains(fiDataFile.Extension.ToLower()))
                {
                    if (mainWindowObject.cbxDelimiter.SelectedIndex != -1)
                    {
                        rows = GetDelimitedFileDataRows();
                        SerializeRows(rows, OutputXMLEncoding);
                    }
                    else
                    {
                        MessageBox.Show("Please select a Delimiter", "Delimiter Missing");
                    }
                }
                else
                    MessageBox.Show("Invalid file extension");
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }


        }
#endregion

#region Serialize Data Rows
        /// <summary>
        /// Serialize Data Rows
        /// </summary>
        /// <param name="rows">The List of Data Rows to be Serialized</param>
        /// <param name="OutputXMLEncoding">The XML Encoding of the output</param>
        private void SerializeRows(List<DataRow> rows, System.Text.Encoding OutputXMLEncoding)
        {
            try
            {
                // Limit the number of Rows as specified
                if (rows != null)
                {
                    rows = LimitRows(rows);

                    // Get the Element Mapping Table and perform the Matching Test
                    DataTable elementMappingTable = ReadElementMapping();
                    PerformMatchingTest(elementMappingTable);

                    // Get the results
                    List<result> results = GetResults(rows, elementMappingTable);

                    if (results != null)
                    {
                        // Create the Message object
                        message oMessage = new message();
                        oMessage.payload = new payload();
                        oMessage.payload.dataset = results.ToArray();

                        // Serialize XML
                        if (Serialize(oMessage, OutputXMLEncoding) == true)
                        {
                            // Validate the produced XML file
                            Validate();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
#endregion

#region Connect to the dbf file and get the Data Rows
        /// <summary>
        /// Connect to the dbf file and get the Data Rows
        /// </summary>
        /// <returns>The list of Data Rows</returns>
        public List<DataRow> GetDataRows()
        {
            try
            {
                mainWindowObject.lblStatus.Content = "Reading the dbf file...";
                DoEvents();

                FileInfo fiDataFile = new FileInfo(mainWindowObject.tbxDataFile.Text);
                String connectionString = @"Provider=VFPOLEDB.1;Data Source=" + mainWindowObject.tbxDataFile.Text;
                OleDbConnection conn = new OleDbConnection(connectionString);
                OleDbDataAdapter da = new OleDbDataAdapter("select * from " + fiDataFile.Name.ToLower().Replace(".dbf", ""), conn);

                DataSet dbfDs = new DataSet();
                da.Fill(dbfDs);
                conn.Close();
                conn.Dispose();
                da.Dispose();
                List<DataRow> rows = (from r in dbfDs.Tables[0].AsEnumerable()
                                      select r).ToList();
                return rows;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Data File Reading problem: " + ex.Message);
                return null;
            }
        }
#endregion

#region Connect to the Delimited file and get the Data Rows
        /// <summary>
        /// Connect to the Delimited file and get the Data Rows
        /// </summary>
        /// <returns>The list of Data Rows</returns>
        public List<DataRow> GetDelimitedFileDataRows()
        {
            try
            {
                mainWindowObject.lblStatus.Content = "Reading the Delimited file...";
                DoEvents();

                DataTable dt = PopulateDataTableFromDelimitedFile(mainWindowObject.tbxDataFile.Text);

                List<DataRow> rows = (from r in dt.AsEnumerable()
                                      select r).ToList();
                return rows;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Data File Reading problem: " + ex.Message);
                return null;
            }
        }
#endregion

#region Limit to the number specified
        /// <summary>
        /// Limit rows to the number specified
        /// </summary>
        /// <param name="rows">The complete list of rows</param>
        /// <returns>The list of rows limited by the specified number</returns>
        protected List<DataRow> LimitRows(List<DataRow> rows)
        {
            Int32 limit = 0;
            if (Int32.TryParse(mainWindowObject.tbxLimit.Text, out limit))
            {
                rows = rows.Take(limit).ToList();
                outputXMLFileName = outputXMLFileName.Replace(".xml", "_First_" + limit.ToString() + ".xml");
            }
            return rows;
        }
#endregion

#region Read the contents of ElementMapping XML into "elementMappingTable"
        /// <summary>
        /// Read the contents of ElementMapping XML into "elementMappingTable"
        /// </summary>
        /// <returns>The Element Mapping Table</returns>
        protected DataTable ReadElementMapping()
        {
#region Read the contents of ElementMapping XML into "elementMappingTable"
            DataTable _elementMappingTable;
            var assembly = Assembly.GetExecutingAssembly();
            Stream elementMappingStream = assembly.GetManifestResourceStream(elementMapping);
            DataSet elementMappingDs = new DataSet();
            elementMappingDs.ReadXml(elementMappingStream);
            _elementMappingTable = elementMappingDs.Tables["ElementMappingEntry"];
            return _elementMappingTable;
#endregion
        }
#endregion

#region Perform a Matching Test to ensure no Schema elements are missing from the Element Mapping File
        /// <summary>
        /// Perform a Matching Test to ensure no Schema elements are missing from the Element Mapping File
        /// </summary>
        /// <param name="elementMappingTable">The Element Mapping Table</param>
        protected void PerformMatchingTest(DataTable elementMappingTable)
        {
            mainWindowObject.lblStatus.Content = "Performing a Matching Test to ensure no Schema elements are missing from the Element Mapping File...";
            DoEvents();

            result matchingTest = new result();
            List<String> elementsNotInMappingFile = new List<String>();

            Type matchingType = matchingTest.GetType();
            PropertyInfo[] matchingProps = matchingType.GetProperties();
            foreach (PropertyInfo p in matchingProps)
                if (GetDbfColumnName(p.Name, elementMappingTable) == null)
                    if (!p.Name.Contains("Specified"))
                        elementsNotInMappingFile.Add(p.Name);

            if (elementsNotInMappingFile.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (String element in elementsNotInMappingFile)
                    sb.Append(element + Environment.NewLine);

                MessageBox.Show("The following schema elements are not in the Mapping File and will be ignored:" + Environment.NewLine + Environment.NewLine + sb.ToString());
            }
        }
#endregion

#region Get the Results
        /// <summary>
        /// Get a list of results to be appended to the dataset object
        /// </summary>
        /// <param name="rows">The Data Rows from the supplied Data File </param>
        /// <param name="elementMappingTable">The Element Mapping Table</param>
        /// <returns></returns>
        protected List<result> GetResults(List<DataRow> rows, DataTable elementMappingTable)
        {
            mainWindowObject.lblStatus.Content = "Generating XML ...";
            DoEvents();


            mainWindowObject.pbrDecoding.Value = 0;
            mainWindowObject.pbrDecoding.Minimum = 0;
            mainWindowObject.pbrDecoding.Maximum = rows.Count;

            try
            {

                List<result> results = new List<result>();

                foreach (DataRow row in rows)
                {
                    mainWindowObject.pbrDecoding.Value = rows.IndexOf(row) + 1;
                    DoEvents();

                    result oResult = new result();

                    Type t = oResult.GetType();
                    PropertyInfo[] props = t.GetProperties();
                    foreach (PropertyInfo p in props)
                        CreateXMLElement(p.Name, row, oResult, elementMappingTable);

                    results.Add(oResult);
                }
                return results;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error getting results: " + ex.Message);
                return null;
            }
        }
#endregion

#region Create the XML Element
        /// <summary>
        /// Create the XML Element
        /// </summary>
        /// <param name="resultElementName">The Name of the XML Element to be created</param>
        /// <param name="row">The DataRow from the DBF file</param>
        /// <param name="oResult">The result object being added</param>
        private void CreateXMLElement(String resultElementName, DataRow row, result oResult, DataTable elementMappingTable)
        {
            // Get the DBF Column name from the codes.xml file
            String dbfColumnName = GetDbfColumnName(resultElementName, elementMappingTable);

            // Ensure that such a DBF Column exists 
            if (dbfColumnName != null && row.Table.Columns.Contains(dbfColumnName))
            {
                // Get the content of the DBF column
                String givenString = row[dbfColumnName].ToString().Trim();
                // If there is conent, create the XML element, otherwise skip
                if (givenString != String.Empty)
                {
                    PropertyInfo prop = oResult.GetType().GetProperty(resultElementName, BindingFlags.Public | BindingFlags.Instance);
                    if (null != prop && prop.CanWrite)
                    {
                        if (prop.PropertyType == typeof(System.String))
                        {
                            prop.SetValue(oResult, givenString, null);
                        }
                        if (prop.PropertyType == typeof(System.Decimal))
                        {
                            Decimal decimalValue;
                            Decimal.TryParse(givenString, out decimalValue);
                            prop.SetValue(oResult, decimalValue, null);
                            PropertyInfo propSpecified = oResult.GetType().GetProperty(resultElementName + "Specified", BindingFlags.Public | BindingFlags.Instance);
                            if (null != propSpecified && propSpecified.CanWrite && decimalValue != 0)
                            {
                                propSpecified.SetValue(oResult, true, null);
                            }
                        }
                        if (prop.PropertyType == typeof(SSDCompoundType))
                        {
                            if (givenString.IndexOf("=") > -1)
                            {
                                // If there is a $ separator, there are multiple attributes
                                string[] attributeArray;
                                if (givenString.IndexOf("$") > -1)
                                {
                                    attributeArray = givenString.Split("$".ToCharArray());
                                }
                                else
                                {
                                    attributeArray = new String[1];
                                    attributeArray[0] = givenString;
                                }

                                List<SSDCompoundTypeValue> values = new List<SSDCompoundTypeValue>();
                                for (int i = 0; i < attributeArray.Length; i++)
                                {
                                    
                                    SSDCompoundTypeValue ctv = new SSDCompoundTypeValue();
                                    ctv.name = attributeArray[i].Split("=".ToCharArray())[0];
                                    ctv.Value = attributeArray[i].Split("=".ToCharArray())[1];
                                    values.Add(ctv);  
                                }
                                SSDCompoundType newSSDCompoundType = new SSDCompoundType();
                                newSSDCompoundType.value = values.ToArray();
                                prop.SetValue(oResult, newSSDCompoundType, null);

                            }
                            else
                            {
                                SSDCompoundType newSSDCompoundType = new SSDCompoundType();
                                newSSDCompoundType.Text = new String[1];
                                newSSDCompoundType.Text[0] = givenString;
                                prop.SetValue(oResult, newSSDCompoundType, null);
                            }

                        }
                        if (prop.PropertyType == typeof(SSDRepeatableType))
                        {
                            SSDRepeatableType newSSDRepeatableType = new SSDRepeatableType();
                            //List<String> rvalues = new List<String>();
                            //rvalues.Add(givenString);
                            //newSSDRepeatableType.value = rvalues.ToArray();
                            newSSDRepeatableType.Text = new String[1];
                            newSSDRepeatableType.Text[0] = givenString;
                            prop.SetValue(oResult, newSSDRepeatableType, null);
                        }
                    }
                }
            }
        }
#endregion

#region Serialize the Message
        /// <summary>
        /// Serialize the Message
        /// </summary>
        /// <param name="oMessage">The Message to serialize</param>
        protected Boolean Serialize(message oMessage, System.Text.Encoding OutputXLMFileEncoding)
        {
#region Serialize and Write
            mainWindowObject.lblStatus.Content = "Writing XML ...";
            DoEvents();

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(message));

                using (FileStream stream = new FileStream(outputXMLFileName, FileMode.Create))
                {
                    // Pesticides SSD1 supports only UTF8
                    XmlTextWriter writer = new XmlTextWriter(stream, OutputXLMFileEncoding);
                    writer.Formatting = Formatting.Indented;
                    writer.Indentation = 4;

                    serializer.Serialize(writer, oMessage);
                    writer.Close();
                }
                mainWindowObject.lblStatus.Content = "XML generated. Now validating ...";
                DoEvents();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("XML Serialization error " + ex.Message);
                return false;
            }

#endregion
        }
#endregion

#region Validate the produced XML file against the XSD
        /// <summary>
        /// Validate the produced XML file against the XSD
        /// </summary>
        private void Validate()
        {
            mainWindowObject.lblStatus.Content = "Validating XML ...";
            DoEvents();

            if (File.Exists("errors.txt"))
                File.Delete("errors.txt");

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
            settings.ValidationEventHandler += new ValidationEventHandler(ValidationCallBack);

            var assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream(xsd);

            XmlSchema schema;
            using (StreamReader sr = new StreamReader(stream))
            {
                var fs = sr.BaseStream;
                schema = XmlSchema.Read(fs, ValidationCallBack);
            }

            settings.Schemas.Add(schema);

            XmlReader reader = XmlReader.Create(outputXMLFileName, settings);
            try
            {
                while (reader.Read()) ;
                if (validationErrorCount == 0)
                    mainWindowObject.lblStatus.Content = "XML generated and validated against the schema.";
                else
                    mainWindowObject.lblStatus.Content = "The XML generated FAILED validation against the schema. Please see the errors file";

            }
            catch (XmlException xex)
            {
                MessageBox.Show(xex.Message);
            }
            reader.Close();
        }

        /// <summary>
        /// Validation CallBack
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ValidationCallBack(object sender, ValidationEventArgs e)
        {
            String errorMessageFileName = "errors.txt";           
            File.AppendAllText(errorMessageFileName, "Failed XSD Validation - " + e.Message + Environment.NewLine);
            validationErrorCount++;
        }
#endregion

#region Get the Dbf Column name for the corresponding XML element name given
        /// <summary>
        /// Returns the Dbf Column name for the corresponding XML element name given
        /// </summary>
        /// <param name="name">The XML Element Name</param>
        /// <returns>The Dbf Column Name</returns>
        private string GetDbfColumnName(String XmlElementName, DataTable elementMappingTable)
        {
            var code = (from c in elementMappingTable.AsEnumerable()
                        where c.Field<String>("XMLElementName") == XmlElementName
                        select c.Field<String>("DbfColumnName")).SingleOrDefault();
            return code;
        }
#endregion

#region Refresh Controls
        /// <summary>
        /// Do Events (refresh controls)
        /// </summary>
        public static void DoEvents()
        {
            try
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
            }
            catch { }
        }

#endregion

#region Populate DataTable From Delimited File
        /// <summary>
        /// Populate DataTable From Delimite.d File
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private DataTable PopulateDataTableFromDelimitedFile(string filePath)
        {
            DataTable dtCSVdata = null;
            TextFieldParser tfParser = new TextFieldParser(filePath);

            try
            {
                ComboBox cb = mainWindowObject.cbxDelimiter;
                ComboBoxItem cbi = (ComboBoxItem)cb.SelectedValue;

                switch (cbi.Name)
                {
                    case "CommaWithQuotes":
                        tfParser.SetDelimiters(new string[] { "," });
                        tfParser.HasFieldsEnclosedInQuotes = true;
                        break;
                    case "Pipe":
                        tfParser.SetDelimiters(new string[] { "|" });
                        break;
                    case "Tab":
                        tfParser.SetDelimiters(new string[] { "\t" });
                        break;
                    case "Semicolon":
                        tfParser.SetDelimiters(new string[] { ";" });
                        break;
                }

                tfParser.TextFieldType = FieldType.Delimited;

                String[] columnHeaders = tfParser.ReadFields();

                DataTable dt = new DataTable();

                foreach (String columnHeader in columnHeaders)
                {
                    DataColumn DataColumn = new DataColumn(columnHeader);
                    DataColumn.AllowDBNull = true;
                    dt.Columns.Add(DataColumn);
                }

                // Iterate through the columns of each row
                int lineNumber = 0;
                while (!tfParser.EndOfData)
                {
                    lineNumber++;
                    String[] columns = tfParser.ReadFields();
                    for (int i = 0; i < columns.Length; i++)
                        if (columns[i] == String.Empty)
                            columns[i] = null;

                    if (columns.Length != columnHeaders.Length)
                    {
                        if (MessageBox.Show("Problem with line " + lineNumber.ToString() + "."
                            + Environment.NewLine
                            + Environment.NewLine
                            + "It possibly contains extra delimiters or it has fields enclosed in Quotes." 
                            + Environment.NewLine
                            + Environment.NewLine
                            + "If you would like to just skip this line and proceed, press OK. " 
                            + Environment.NewLine
                            + "If you would like to stop the XML generation, press Cancel",
                                "Problems with line " + lineNumber.ToString() + "."
                                , MessageBoxButton.OKCancel
                                , MessageBoxImage.Warning) == MessageBoxResult.Cancel)
                            break;
                    }
                    else
                        dt.Rows.Add(columns);
                }
                dtCSVdata = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
                mainWindowObject.lblStatus.Content = String.Empty;
                mainWindowObject.pbrDecoding.Value = 0;
            }
            return dtCSVdata;
        }
#endregion

    }
}
