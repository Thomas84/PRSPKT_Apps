using Autodesk.Revit.DB;
using PRSPKT_Apps.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRSPKT_Apps.ExportManager
{
    public class ExportManager
    {
        private static Dictionary<string, FamilyInstance> _titleBlocks;
        private static Document _doc;
        private static string _activeDoc;
        private ExportOptions _exportFlags;
        private ExportLog _log;
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
            this._log = new ExportLog();
            this._allViewSheetSets = new Collection<ViewSheetSetCombo>();
            this._fileNameTypes = new SortableBindingListCollection<ExportSheet>();
            this._postExportHooks = new Dictionary<string, PostExportHookCommand>();
            this._exportFlags = ExportOptions.None;
            this.LoadSettings();
            this.SetDefaultFlags();
            ExportManager.PopulateViewSheetSets(this._allViewSheetSets);
            this.PopulateSheets(this._allSheets);
            ExportManager.FixAcrotrayHang();
        }

        public static bool ConfirmOverwrite { get; set; }
        public string PrinterNameA3 { get; set; }
        public string PrinterNameLargeFormat { get; set; }
        public string PdfPrinterName { get; set; }
        public string PostscriptPrinterName { get; set; }
        public string GhostscriptLibDirectory { get; set; }
        public string GhostscriptBinDirectory { get; set; }
        public Collection<SegmentedSheetName> FileNameTypes { get => _fileNameTypes; }
        public Collection<ViewSheetSetCombo> AllViewSheetSets { get => _allViewSheetSets; }
        public SortableBindingListCollection<ExportSheet> AllSheets { get => _allSheets; }
        public ExportOptions ExportOptions {get; set;}
        public ACADVersion AcadVersion { get; set; }
        public bool ForceRevisionToDateString
        {
            get => this._forceDate;
            set
            {
                this._forceDate = value;
                foreach (ExportSheet sheet in _allSheets)
                {
                    sheet.ForceDate = value;
                }
            }
        }

        public string ExportDirectory
        {
            get => this._exportDirectory;
            set
            {
                if (value != null)
                {
                    _exportDirectory = value;
                    foreach (ExportSheet sheet in _allSheets)
                    {
                        sheet.ExportDirectory = value;
                    }
                }
            }
        }

        public SegmentedSheetName FileNameScheme { get => _fileNameScheme; }
        public bool ShowExportLog { get; set; }
        public static FamilyInstance TitleBlockInstanceFromSheetNumber(
            string sheetNumber, Document doc)
        {
            if (doc == null)
            {
                return null;
            }

            FamilyInstance result;
            if (_titleBlocks == null || _activeDoc != FileUtilities.GetCentralFileName(doc))
            {
                _activeDoc = FileUtilities.GetCentralFileName(doc);
                _titleBlocks = AllTitleBlocks(doc);
            }

            if (_titleBlocks.TryGetValue(sheetNumber, out result)
            {
                return result;
            }
            else
            {
                _titleBlocks = AllTitleBlocks(doc);
            }
            return _titleBlocks.TryGetValue(sheetNumber, out result) ? result : null;
        }

        public static string CreateExportConfig(Document doc)
        {
            string fileName = GetConfigFileName(doc);
            return File.Exists(fileName) ? fileName : null;
        }

        public static string GetOldConfigFileName(Document doc)
        {
            string centralFileName = FileUtilities.GetCentralFileName(doc);
            string s = Path.GetDirectoryName(centralFileName) + Path.DirectorySeparatorChar +
                Path.GetFileNameWithoutExtension(centralFileName) + Resources.FileExtensionXML;
        }



    }
}
