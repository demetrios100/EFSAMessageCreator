#region using 
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
#endregion


namespace EFSAMessageCreator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        protected System.Text.Encoding OutputXMLEncoding = System.Text.Encoding.Unicode;


        #region Initialization

        public MainWindow()
        {
            InitializeComponent();

            this.Title = App.ApplicationTitle;
            AccessText text = new AccessText();
            text.TextWrapping = TextWrapping.Wrap;
            text.TextAlignment = TextAlignment.Center;
            text.Text = App.ApplicationTitle;
            appBanner.Content = text;
            Uri iconUri = new Uri("pack://application:,,,/" + App.ApplicationIcon, UriKind.RelativeOrAbsolute);
            this.Icon = BitmapFrame.Create(iconUri);
            var bc = new BrushConverter();
            this.Background = (Brush)bc.ConvertFrom(App.ApplicationBackground);
            lblSchema.Content = "Schema: " + App.Schema;


            if (ConfigurationManager.AppSettings["datapath"] != null)
                tbxDataFile.Text = ConfigurationManager.AppSettings["datapath"];

            ComboBoxItem cbi;
            cbi = new ComboBoxItem
            {
                Name = "CommaWithQuotes",
                Content = "Comma ,"
            };
            cbxDelimiter.Items.Add(cbi);

            cbi = new ComboBoxItem
            {
                Name = "Semicolon",
                Content = "Semicolon ;"
            };
            cbxDelimiter.Items.Add(cbi);

            cbi = new ComboBoxItem
            {
                Name = "Tab",
                Content = "Tab"
            };
            cbxDelimiter.Items.Add(cbi);

            cbi = new ComboBoxItem
            {
                Name = "Pipe",
                Content = "Pipe (Vertical Bar) |"
            };
            cbxDelimiter.Items.Add(cbi);

        }

        #endregion

        #region Select the Data File
        /// <summary>
        /// Select the DBF File
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = "Text files(*.txt) |*.txt| CSV Text files(*.csv) |*.csv| DBF files(*.dbf)|*.dbf| All files(*.*) | *.*";
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            tbxDataFile.Text = dialog.FileName;

            if (ConfigurationManager.AppSettings["datapath"] == null)
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings.Add("datapath", dialog.FileName);
                config.Save(ConfigurationSaveMode.Full);
                ConfigurationManager.RefreshSection("appsettings");
            }
            else
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings["datapath"].Value = dialog.FileName;
                config.Save(ConfigurationSaveMode.Full);
                ConfigurationManager.RefreshSection("appsettings");
            }
        }
        #endregion

        #region Button creating XML
        /// <summary>
        /// Button creating XML
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCreateXML_Click(object sender, RoutedEventArgs e)
        {
                try
                {
                    CreateXML();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
        }
        #endregion

        #region Create XML
        /// <summary>
        /// Create XML
        /// </summary>
        private void CreateXML()
        {
            String XSDFileFullName = "EFSAMessageCreator." + App.Schema.Trim();
            String ElementMappingFileFullName = "EFSAMessageCreator." + App.ElementMappingFileName.Trim();

            XMLGeneration xmlGeneration
                = new XMLGeneration(
                    this,
                    XSDFileFullName,
                    ElementMappingFileFullName,
                    App.OutputXMLFileName,
                    OutputXMLEncoding);
        }
        #endregion

        #region Limit CheckBox Handlers
        private void cbxLimit_Checked(object sender, RoutedEventArgs e)
        {
            tbxLimit.Visibility = Visibility.Visible;
            cbxLimit.Content = "For testing, limit the number of records generated to ";
        }

        private void cbxLimit_Unchecked(object sender, RoutedEventArgs e)
        {
            tbxLimit.Visibility = Visibility.Collapsed;
            tbxLimit.Text = String.Empty;
            cbxLimit.Content = "For testing, limit the number of records generated";
        }

        #endregion

        #region Closing
        private void Window_Closed(object sender, EventArgs e)
        {
            //Environment.Exit(0);
            Application.Current.Shutdown();
        }
        #endregion

        #region Delimited Options
        private void tbxDataFile_TextChanged(object sender, TextChangedEventArgs e)
        {
            DetermineCSVOptionsDisplay();
        }

        private void tbxDataFile_Loaded(object sender, RoutedEventArgs e)
        {
            DetermineCSVOptionsDisplay();
        }

        private void DetermineCSVOptionsDisplay()
        {
            String[] csvExtensions = { ".csv", ".txt" };

            if (tbxDataFile.Text != String.Empty)
            {
                try
                {
                    FileInfo fiDataFile = new FileInfo(tbxDataFile.Text);
                    if (fiDataFile.Extension.ToLower() == ".dbf")
                    {
                        pnlDelimitedOptions.Visibility = Visibility.Collapsed;
                    }
                    else if (csvExtensions.Contains(fiDataFile.Extension.ToLower()))
                    {
                        pnlDelimitedOptions.Visibility = Visibility.Visible;
                    }
                }
                catch
                {
                    pnlDelimitedOptions.Visibility = Visibility.Collapsed;
                }
            }
        }
        #endregion

    }
}
