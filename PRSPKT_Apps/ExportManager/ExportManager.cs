using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using PRSPKT_Apps.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Xml.Schema;

namespace PRSPKT_Apps.ExportManager
{
    public class ExportManager
    {

        private static Dictionary<string, FamilyInstance> _titleBlocks;
        private static Document _doc;
        private static string _activeDoc;
        private ExportOptions _exportFlags;
        // private ExportLog _log;
        private Collection<SegmentedSheetName> _fileNameTypes;
        private Collection<ViewSheetSetCombo> _allViewSheetSets;
        private Dictionary<string, PostExportHookCommand> _postExportHooks;
        private SegmentedSheetName _fileNameScheme;
        private SortableBindingListCollection<ExportSheet> _allSheets;
        private bool _forceDate;
        private string _exportDirectory;

        public ExportManager(Document doc)
        {
            ExportManager._doc = doc;
            this._fileNameScheme = null;
            this._exportDirectory = Constants.DefaultExportDirectory;
            ExportManager.ConfirmOverwrite = true;
            ExportManager._activeDoc = null;
            //this._log = new ExportLog();
            this._allViewSheetSets = new Collection<ViewSheetSetCombo>();
            this._allSheets = new SortableBindingListCollection<ExportSheet>();
            this._fileNameTypes = new Collection<SegmentedSheetName>();
            this._postExportHooks = new Dictionary<string, PostExportHookCommand>();
            this._exportFlags = ExportOptions.None;
            this.LoadSettings();
            this.SetDefaultFlags();
            PopulateViewSheetSets(this._allViewSheetSets);
            this.PopulateSheets(this._allSheets);
            ExportManager.FixAcrotrayHang();
        }

        private static void FixAcrotrayHang()
        {
            Microsoft.Win32.Registry.SetValue(Constants.HungAppTimeout, "HungAppTimeout", "30000", Microsoft.Win32.RegistryValueKind.String);
        }

        public static bool ConfirmOverwrite { get; set; }

        public static string PrinterNameA3 { get; set; }

        public string PrinterNameLargeFormat { get; set; }
        public string PdfPrinterName { get; set; }
        public string PostScriptPrinterName { get; set; }
        public string GhostscriptLibDirectory { get; set; }
        public string GhostscriptBinDirectory { get; set; }
        public Collection<SegmentedSheetName> FileNamesTypes { get => _fileNameTypes; }
        public SortableBindingListCollection<ExportSheet> AllSheets { get => _allSheets; }
        public Collection<ViewSheetSetCombo> AllViewSheetSets { get => _allViewSheetSets; }
        public ExportOptions ExportOptions { get; set; }
        public ACADVersion AcadVersion { get; set; }
        public bool ForceRevisionToDateString
        {
            get => _forceDate;
            set
            {
                _forceDate = value;
                foreach (ExportSheet sheet in _allSheets)
                {
                    sheet.ForceDate = value;
                }
            }
        }

        public string ExportDirectory
        {
            get
            {
                return this._exportDirectory;
            }

            set
            {
                if (value != null)
                {
                    this._exportDirectory = value;
                    foreach (ExportSheet sheet in this._allSheets)
                    {
                        sheet.ExportDirectory = value;
                    }
                }
            }
        }

        public SegmentedSheetName FileNameScheme
        {
            get
            {
                return this._fileNameScheme;
            }
        }

        public bool ShowExportLog { get; set; }
        public static FamilyInstance TitleBlockInstanceFromSheetNumber(
            string sheetNumber, Document doc)
        {
            if (doc == null)
            {
                return null;
            }

            if ((_titleBlocks == null) || (_activeDoc != MiscUtils.GetCentralFileName(doc)))
            {
                _activeDoc = MiscUtils.GetCentralFileName(doc);
                _titleBlocks = AllTitleBlocks(doc);
            }

            if (_titleBlocks.TryGetValue(sheetNumber, out FamilyInstance result))
            {
                return result;
            }
            else _titleBlocks = AllTitleBlocks(doc);

            return _titleBlocks.TryGetValue(sheetNumber, out result) ? result : null;
        }

        private static Dictionary<string, FamilyInstance> AllTitleBlocks(Document doc)
        {
            var result = new Dictionary<string, FamilyInstance>();
            using (var collector = new FilteredElementCollector(doc))
            {
                collector.OfCategory(BuiltInCategory.OST_TitleBlocks);
                collector.OfClass(typeof(FamilyInstance));
                foreach (FamilyInstance item in collector)
                {
                    var num = item.get_Parameter(BuiltInParameter.SHEET_NUMBER).AsString();
                    if (!result.ContainsKey(num))
                    {
                        result.Add(num, item);
                    }
                }
            }
            return result;
        }

        public static string CreatePRSPKTconfig(Document doc)
        {
            string s = MiscUtils.GetConfigFileName(doc);
            return File.Exists(s) ? s : null;
        }

        public static string GetOldConfigFileName(Document doc)
        {
            string central = MiscUtils.GetCentralFileName(doc);
            string s = Path.GetDirectoryName(central) + Path.DirectorySeparatorChar +
                Path.GetFileNameWithoutExtension(central) + Tools.GetResourceManager("FileExtensionXML");
            return s;
        }

        public static string CurrentViewName()
        {
            View v = _doc.ActiveView;
            if (v.ViewType == ViewType.DrawingSheet)
            {
                return v.get_Parameter(BuiltInParameter.SHEET_NUMBER).AsString();
            }
            else return null;
        }

        public static ACADVersion AcadVersionFromString(string version)
        {
            if (version == "R2007")
            {
                return ACADVersion.R2007;
            }
            if (version == "R2010")
            {
                return ACADVersion.R2010;
            }
            return (version == "R2013") ? ACADVersion.R2013 : ACADVersion.Default;
        }

        public static string AcadVersionToString(ACADVersion version)
        {
            switch (version)
            {
                case ACADVersion.R2007:
                    return "R2007";
                case ACADVersion.R2010:
                    return "R2010";
                case ACADVersion.R2013:
                    return "R2013";
                default:
                    return "Default";
            }
        }

        public static string GetLatestRevisionDate()
        {
            string s = "";
            int i = -1;
            using (FilteredElementCollector collector = new FilteredElementCollector(_doc))
            {
                collector.OfCategory(BuiltInCategory.OST_Revisions);
                foreach (Element e in collector)
                {
                    int j = e.get_Parameter(BuiltInParameter.PROJECT_REVISION_SEQUENCE_NUM).AsInteger();
                    if (j > i)
                    {
                        i = j;
                        s = e.get_Parameter(BuiltInParameter.PROJECT_REVISION_REVISION_DATE).AsString();
                    }
                }
            }
            return (s.Length > 1) ? s : "";
        }

        private static void PopulateViewSheetSets(Collection<ViewSheetSetCombo> vssc)
        {
            vssc.Clear();
            using (FilteredElementCollector collector = new FilteredElementCollector(_doc))
            {
                collector.OfClass(typeof(ViewSheetSet));
                foreach (ViewSheetSet v in collector)
                {
                    vssc.Add(new ViewSheetSetCombo(v));
                }
            }
        }

        public static string GetConfigFileName(Document doc)
        {
            string central = MiscUtils.GetCentralFileName(_doc);
            string path = Path.GetDirectoryName(central) + @"\PPRINTexport.xml";
            return path;
        }

        private bool ValidateXML(string filename)
        {
            string errorMessage = "";
            if (filename == null || !File.Exists(filename))
            {
                return false;
            }
            try
            {
                var settings = new System.Xml.XmlReaderSettings();
                settings.Schemas.Add(null, Constants.InstallDir + @"\etc\PPRINTexport.xsd");
                settings.ValidationType = System.Xml.ValidationType.Schema;
                var reader = System.Xml.XmlReader.Create(filename, settings);
                var document = new System.Xml.XmlDocument();
                document.Load(reader);
                var eventHandler = new System.Xml.Schema.ValidationEventHandler(
                    ValidationEventHandler);
                document.Validate(eventHandler);
                return true;
            }
            catch (System.Xml.Schema.XmlSchemaValidationException ex)
            {
                errorMessage += "Error reading xml file:" + filename + " - " + ex.Message;
            }
            catch (System.Xml.XmlException ex)
            {
                errorMessage += "Error reading xml file:" + filename + " - " + ex.Message;
            }
            catch (System.Xml.Schema.XmlSchemaException ex)
            {
                errorMessage += "Error reading xml file:" + filename + " - " + ex.Message;
            }
            catch (System.ArgumentNullException ex)
            {
                errorMessage += "Error reading xml file:" + filename + " - " + ex.Message;
            }
            catch (System.UriFormatException ex)
            {
                errorMessage += "Error reading xml file:" + filename + " - " + ex.Message;
            }
            using (var td = new TaskDialog("PPRINTexport - XML Config error"))
            {
                td.MainIcon = TaskDialogIcon.TaskDialogIconWarning;
                td.MainInstruction = "PPRINTexport - XML Config error";
                td.MainContent = errorMessage;
                td.Show();
            }
            return false;
        }

        private void ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            switch (e.Severity)
            {
                case XmlSeverityType.Error:
                    TaskDialog.Show("Error: {0}", e.Message);
                    break;
                case XmlSeverityType.Warning:
                    TaskDialog.Show("Error: {0}", e.Message);
                    break;
            }
        }

        private bool ImportXMLinfo(string filename)
        {
            if (!File.Exists(filename))
            {
                return false;
            }
            if (!ValidateXML(filename))
            {
                return false;
            }

            using (var reader = new System.Xml.XmlTextReader(filename))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == System.Xml.XmlNodeType.Element
                        && reader.Name == "PostExportHook")
                    {
                        var hook = new PostExportHookCommand();
                        if (reader.AttributeCount > 0)
                        {
                            hook.SetName(reader.GetAttribute("name"));
                        }
                        do
                        {
                            reader.Read();
                            if (reader.NodeType == System.Xml.XmlNodeType.Element)
                            {
                                switch (reader.Name)
                                {
                                    case "Command": hook.SetCommand(reader.ReadString()); break;
                                    case "Args": hook.SetArguments(reader.ReadString()); break;
                                    case "SupportedFileExtension": hook.AddSupportedFilenameExtension(reader.ReadString()); break;
                                }
                            }
                        }
                        while (!(reader.NodeType == System.Xml.XmlNodeType.EndElement &&
                        reader.Name == "PostExportHook"));
                        _postExportHooks.Add(hook.Name, hook);
                    }
                    if (reader.NodeType == System.Xml.XmlNodeType.Element && reader.Name == "FilenameScheme")
                    {
                        var name = new SegmentedSheetName();
                        if (reader.AttributeCount > 0)
                        {
                            name.Name = reader.GetAttribute("name");
                        }
                        do
                        {
                            reader.Read();
                            if (reader.NodeType == System.Xml.XmlNodeType.Element)
                            {
                                switch (reader.Name)
                                {
                                    case "Format": name.NameFormat = reader.ReadString(); break;
                                    case "Hook": name.Hooks.Add(reader.ReadString()); break;
                                }
                            }
                        } while (!(reader.NodeType == System.Xml.XmlNodeType.EndElement && reader.Name == "FilenameScheme"));
                        FileNamesTypes.Add(name);
                    }
                }
                if (FileNamesTypes.Count > 0)
                {
                    _fileNameScheme = FileNamesTypes[0];
                    foreach (ExportSheet sheet in AllSheets)
                    {
                        sheet.SetSegmentedSheetName(FileNameScheme);
                    }
                }
            }
            return true;
        }


        private void PopulateSheets(SortableBindingListCollection<ExportSheet> es)
        {
            string config = GetConfigFileName(_doc);
            bool b = ImportXMLinfo(config);
            if (!b)
            {
                var name = new SegmentedSheetName()
                {
                    Name = "YYYY-MM-DD_AD_NNN",
                    NameFormat = "$projectNumber-$sheetNumber[$sheetRevision]"
                };
                FileNamesTypes.Add(name);
                _fileNameScheme = name;
            }
            if (FileNamesTypes.Count <= 0)
            {
                var name = new SegmentedSheetName()
                {
                    Name = "YYYY-MM-DD_AD-NNN",
                    NameFormat = "$projectNumber=$sheetNumber[$sheetRevision]"
                };
                FileNamesTypes.Add(name);
                _fileNameScheme = name;
            }

            es.Clear();
            using (FilteredElementCollector collector = new FilteredElementCollector(_doc))
            {
                collector.OfCategory(BuiltInCategory.OST_Sheets);
                collector.OfClass(typeof(ViewSheet));
                foreach (ViewSheet v in collector)
                {
                    var pSheet = new ExportSheet(v, _doc, FileNamesTypes[0], this);
                    es.Add(pSheet);
                }
            }
        }
        public void LoadSettings()
        {
            GhostscriptBinDirectory = Settings1.Default.GSBinDirectory;
            PdfPrinterName = Settings1.Default.AdobePrinterDriver;
            PrinterNameA3 = Settings1.Default.A3PrinterDriver;
            PrinterNameLargeFormat = Settings1.Default.LargeFormatPrinterDriver;
            PostScriptPrinterName = Settings1.Default.PSPrinterDriver;
            GhostscriptLibDirectory = Settings1.Default.GSLibDirectory;
            ExportDirectory = Settings1.Default.ExportDir;
            AcadVersion = AcadVersionFromString(Settings1.Default.AcadExportVersion);
            ShowExportLog = Settings1.Default.ShowExportLog;

        }

        private void SetDefaultFlags()
        {
            if (Settings1.Default.AdobePDFMode && PDFSanityCheck())
            {
                AddExportOption(ExportOptions.PDF);
            }
            else if (Settings1.Default.AdobePDFMode && GSSanityCheck())
            {
                AddExportOption(ExportOptions.GhostscriptPDF);
            }
            else
            {
                if (PDFSanityCheck())
                {
                    AddExportOption(ExportOptions.PDF);
                }
                AddExportOption(ExportOptions.DWG);
            }
            if (Settings1.Default.HideTitleBlocks)
            {
                AddExportOption(ExportOptions.NoTitle);
            }
            _forceDate = Settings1.Default.ForceDateRevision;
        }

        public bool GSSanityCheck()
        {
            var printerSettings = new System.Drawing.Printing.PrinterSettings()
            {
                PrinterName = PostScriptPrinterName
            };
            return printerSettings.IsValid;
        }

        public bool PDFSanityCheck()
        {
            var printerSettings = new System.Drawing.Printing.PrinterSettings()
            {
                PrinterName = PdfPrinterName
            };
            return printerSettings.IsValid;
        }

        public void AddExportOption(ExportOptions exportOpts)
        {
            _exportFlags |= exportOpts;
        }

        public void RemoveExportOption(ExportOptions exportOpts)
        {
            _exportFlags = _exportFlags & ~exportOpts;
        }


            [SecurityCritical]
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        private static void SetAcrobatExportRegistryVal(string fileName, ExportLog log)
        {
            string exe =
                Process.GetCurrentProcess().MainModule.FileName;
            try
            {
                log.AddMessage("Attempting to set Acrobat Registry Value with value");
                log.AddMessage("\t" + Constants.AcrobatPrinterJobControl);
                log.AddMessage("\t" + exe);
                log.AddMessage("\t" + fileName);
                Microsoft.Win32.Registry.SetValue(
                    Constants.AcrobatPrinterJobControl,
                    exe,
                    fileName,
                    Microsoft.Win32.RegistryValueKind.String);
            }
            catch (UnauthorizedAccessException ex)
            {
                log.AddError(fileName, ex.Message);
            }
            catch (System.Security.SecurityException ex)
            {
                log.AddError(fileName, ex.Message);
            }
        }
    }
}