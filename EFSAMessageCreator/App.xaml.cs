using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace EFSAMessageCreator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Variables to be changed when a new Schema is used
        

#if Debug_Pesticides
        public static String Schema = "WF2_pesticides_SSD1.xsd";
        public static String ElementMappingFileName = "ElementMappingPesticidesSSD1.xml";
        public static String OutputXMLFileName = "Pesticides.xml";
        public static String ApplicationTitle = "EFSA Message Creator for Pesticides (SSD1)";
        public static String ApplicationIcon = "Pesticides.ico";
        public static String ApplicationBackground = "#FFB0CD94";
#elif Debug_Chemicals
        public static String Schema = "Chemicals_XSD_20180405.xsd";
        public static String ElementMappingFileName = "ElementMappingChemicals.xml";
        public static String OutputXMLFileName = "Chemicals.xml";
        public static String ApplicationTitle = "EFSA Message Creator for Chemicals";
        public static String ApplicationIcon = "Chemicals.ico";
        public static String ApplicationBackground = "#FFC9B4E6";
#elif Debug_Veterinary
        public static String Schema = "VMPR_WF2_schema.xsd";
        public static String ElementMappingFileName = "ElementMappingVeterinary.xml";
        public static String OutputXMLFileName = "Veterinary.xml";
        public static String ApplicationTitle = "EFSA Message Creator for Veterinary";
        public static String ApplicationIcon = "Veterinary.ico";
        public static String ApplicationBackground = "#FFCBC299";
#elif Release_Pesticides
        public static String Schema = "WF2_pesticides_SSD1.xsd";
        public static String ElementMappingFileName = "ElementMappingPesticidesSSD1.xml";
        public static String OutputXMLFileName = "Pesticides.xml";
        public static String ApplicationTitle = "EFSA Message Creator for Pesticides (SSD1)";
        public static String ApplicationIcon = "Pesticides.ico";
        public static String ApplicationBackground = "#FFB0CD94";
#elif Release_Chemicals
        public static String Schema = "Chemicals_XSD_20180405.xsd";
        public static String ElementMappingFileName = "ElementMappingChemicals.xml";
        public static String OutputXMLFileName = "Chemicals.xml";
        public static String ApplicationTitle = "EFSA Message Creator for Chemicals";
        public static String ApplicationIcon = "Chemicals.ico";
        public static String ApplicationBackground = "#FFC9B4E6";
#elif Release_Veterinary
        public static String Schema = "VMPR_WF2_schema.xsd";
        public static String ElementMappingFileName = "ElementMappingVeterinary.xml";
        public static String OutputXMLFileName = "Veterinary.xml";
        public static String ApplicationTitle = "EFSA Message Creator for Veterinary";
        public static String ApplicationIcon = "Veterinary.ico";
        public static String ApplicationBackground = "#FFCBC299";
#endif



    }
}
