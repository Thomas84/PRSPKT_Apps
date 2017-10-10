namespace PRSPKT_Apps.ExportManager
{
    public static class Constants
    {
        /// <summary>
        /// The acrobat Printer Job Control Registry settings
        /// </summary>
        public const string AcrobatPrinterJobControl =
            @"HKEY_CURRENT_USER\Software\Adobe\Acrobat Distiller\PrinterJobControl";

        /// <summary>
        /// The hung app timeout.
        /// This is the amount of time that windows waits before exiting
        /// a non-responsive program.
        /// </summary>
        public const string HungAppTimeout =
            @"HKEY_CURRENT_USER\Control Panel\Desktop";

        /// <summary>
        ///  Example project configuration file
        /// </summary>
        public const string ExampleConfigFileName =
            "PRSPKT_apps-example-conf.xml";

        /// <summary>
        /// The export directory by default.
        /// </summary>
        public const string DefaultExportDirectory =
            @"%TEMP%";

        /// <summary>
        /// Param name of scale bar visibility
        /// </summary>
        public const string TitleScale = "1 к";

        /// <summary>
        /// PRSPKTexport registry path.
        /// </summary>
        public const string RegistryPath =
            @"HKEY_CURRENT_USER\Software\PRSPKT_Apps\PRSPKTexport";
    }
}
