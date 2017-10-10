using Autodesk.Revit.DB;
using PRSPKT_Apps.Common;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

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
            ExportManager.PopulateViewSheetSets(this._allViewSheetSets);
            this.PopulateSheets(this._allSheets);
            ExportManager.FixAcrotrayHang();
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

            FamilyInstance result;
            if ((_titleBlocks == null) || (_activeDoc != MiscUtils.GetCentralFileName(doc))
            {
                _activeDoc = MiscUtils.GetCentralFileName(doc);
                _titleBlocks = AllTitleBlocks(doc);
            }

            if (_titleBlocks.TryGetValue(sheetNumber, out result))
            {
                return result;
            }
            else _titleBlocks = AllTitleBlocks(doc);

            return _titleBlocks.TryGetValue(sheetNumber, out result) ? result : null;
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
            var printerSettings = new System.Drawing.Printing.PrinterSettings();
            printerSettings.PrinterName = PostScriptPrinterName;
            return printerSettings.IsValid;
        }

        public bool PDFSanityCheck()
        {
            var printerSettings = new System.Drawing.Printing.PrinterSettings();
            printerSettings.PrinterName = PdfPrinterName;
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

    }
}