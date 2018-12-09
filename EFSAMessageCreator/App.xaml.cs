// <copyright file="App.xaml.cs" company="EFSAUsersGroup">Copyright (c) EFSA Users Group. All rights reserved.</copyright>
// <author>Demetrios Ioannides</author>
// <email>dvi1@columbia.edu</email>
// <summary>The App xaml for the EFSA Message Creator Utility</summary>

namespace EFSAMessageCreator
{
    using System;
    using System.Windows;

    /// <summary>
    /// Interaction logic for App XAML
    /// </summary>
    public partial class App : Application
    {
        // Variables to be changed when a new Schema is used      
        #if Debug_Pesticides
            public static String Schema = "WF2_pesticides_SSD2.xsd";
            public static String ElementMappingFileName = "ElementMapping.xml";
            public static String OutputXMLFileName = "Pesticides.xml";
            public static String ApplicationTitle = "EFSA Message Creator for Pesticides";
            public static String ApplicationIcon = "Pesticides.ico";
            public static String ApplicationBackground = "#FFB0CD94";
        #elif Debug_Chemicals
            public static String Schema = "Chemicals_XSD_20180405.xsd";
            public static String ElementMappingFileName = "ElementMapping.xml";
            public static String OutputXMLFileName = "Chemicals.xml";
            public static String ApplicationTitle = "EFSA Message Creator for Chemicals";
            public static String ApplicationIcon = "Chemicals.ico";
            public static String ApplicationBackground = "#FFC9B4E6";
        #elif Debug_Veterinary
            public static String Schema = "VMPR_WF2_schema.xsd";
            public static String ElementMappingFileName = "ElementMapping.xml";
            public static String OutputXMLFileName = "Veterinary.xml";
            public static String ApplicationTitle = "EFSA Message Creator for Veterinary";
            public static String ApplicationIcon = "Veterinary.ico";
            public static String ApplicationBackground = "#FFCBC299";
        #elif Release_Pesticides
            public static String Schema = "WF2_pesticides_SSD2.xsd";
            public static String ElementMappingFileName = "ElementMapping.xml";
            public static String OutputXMLFileName = "Pesticides.xml";
            public static String ApplicationTitle = "EFSA Message Creator for Pesticides";
            public static String ApplicationIcon = "Pesticides.ico";
            public static String ApplicationBackground = "#FFB0CD94";        
        #elif Release_Chemicals
            public static String Schema = "Chemicals_XSD_20180405.xsd";
            public static String ElementMappingFileName = "ElementMapping.xml";
            public static String OutputXMLFileName = "Chemicals.xml";
            public static String ApplicationTitle = "EFSA Message Creator for Chemicals";
            public static String ApplicationIcon = "Chemicals.ico";
            public static String ApplicationBackground = "#FFC9B4E6";
        #elif Release_Veterinary
            public static String Schema = "VMPR_WF2_schema.xsd";
            public static String ElementMappingFileName = "ElementMapping.xml";
            public static String OutputXMLFileName = "Veterinary.xml";
            public static String ApplicationTitle = "EFSA Message Creator for Veterinary";
            public static String ApplicationIcon = "Veterinary.ico";
            public static String ApplicationBackground = "#FFCBC299";
        #endif
    }
}
