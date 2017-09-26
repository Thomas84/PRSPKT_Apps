using Autodesk.Revit.DB;
using PRSPKT_Apps.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRSPKT_Apps.ExportManager
{
    public class ExportSheet
    {
        #region Variables
        private DateTime _sheetRevisionDateTime;
        private Document _doc;
        private ElementId _id;
        private PrintSetting _printSetting;
        private SegmentedSheetName _segmentedFileName;
        private ViewSheet _sheet;
        private bool _forceDate;
        private bool _verified;
        private int _northPointVisible;
        private double _height;
        private double _width;
        private string _fullExportName;
        private string _pageSize;
        private string _projectNumber;
        private string _scale;
        private string _scaleBarScale;
        private string _sheetDescription;
        private string _sheetNumber;
        private string _sheetRevision;
        private string _sheetRevisionDate;
        private string _sheetRevisionDescription;
        public ExportSheet(
            ViewSheet sheet,
            Document doc,
            SegmentedSheetName fileNameTemplate,
            ExportSheet expMan)
        {
            this.Init(sheet, doc, fileNameTemplate, expMan);
        }

        private void Init(ViewSheet sheet, Document doc, SegmentedSheetName fileNameTemplate, ExportSheet expMan)
        {
            this._doc = doc;
            this._sheet = sheet;
            this._segmentedFileName = fileNameTemplate;
            this._verified = false;
            this.ExportDirectory = expMan.ExportDirectory;
            this._sheetNumber = sheet.get_Parameter(BuiltInParameter.SHEET_NUMBER).AsString();
            this._sheetDescription = sheet.get_Parameter(BuiltInParameter.SHEET_NAME).AsString();
            this._projectNumber = doc.ProjectInformation.Number;
            this._width = 841;
            this._height = 594;
            this._scale = string.Empty;
            this._scaleBarScale = string.Empty;
            this._northPointVisible = 2;
            this._pageSize = string.Empty;
            this._id = sheet.Id;
            this.UpdateRevision(false);
            this.SetExportName();
        }

        private void UpdateRevision(bool refreshExportName)
        {
            this._sheetRevision = this._sheet.get_Parameter(BuiltInParameter.SHEET_CURRENT_REVISION).AsString();
            this._sheetRevisionDescription = this._sheet.get_Parameter(BuiltInParameter.SHEET_CURRENT_REVISION_DESCRIPTION).AsString();
            this._sheetRevisionDate = this._sheet.get_Parameter(BuiltInParameter.SHEET_CURRENT_REVISION_DATE).AsString();
            this._sheetRevisionDateTime = PRSPKT_Apps.Common.MiscUtils.ToDateTime(this._sheetRevisionDate);
            if (refreshExportName)
            {
                this.SetExportName();
            }
        }

        public string PageSize { get => _pageSize; set => _pageSize = value; }
        public string ProjectNumber { get => _projectNumber; set => _projectNumber = value; }
        public PrintSetting PrintSetting { get => _printSetting; }
        public SegmentedSheetName SegmentedFileName { get => _segmentedFileName; set => _segmentedFileName = value; }
        public string SheetNumber
        {
            get { return _sheetNumber; }
            set
            {
                this._sheetNumber = value;
                this.SetExportName();
            }
        }
        public string SheetRevision { get => _sheetRevision ?? "-"; set => _sheetRevision = value; }
        public string SheetRevisionDate { get => _sheetRevisionDate ?? "-"; set => _sheetRevisionDate = value; }
        public string SheetRevisionDescription { get => _sheetRevisionDescription ?? "-"; set => _sheetRevisionDescription = value; }
        public DateTime SheetRevisionDateTime { get => _sheetRevisionDateTime; set => _sheetRevisionDateTime = value; }
        public string Scale
        {
            get
            {
                string result = this._scale.Trim();
                int i = 0;
                if (result.Contains(":"))
                {
                    i = result.IndexOf(':');
                }
                bool flag = false;
                if (string.IsNullOrEmpty(result.Trim()))
                {
                    result = "0";
                }
                if (!string.IsNullOrEmpty(this._scaleBarScale))
                {
                    flag |= i > 0 && !result.Substring(i + 2).Equals(this._scaleBarScale.Trim());
                }
                if (!string.IsNullOrEmpty(this._scaleBarScale.Trim()) && flag)
                {
                    result += " [**" + this._scaleBarScale + "]";
                }
                return result;
            }
        }
        public double Width { get => MiscUtils.FeetToMillimeters(_width);  set => _width = value; }
        public double Height { get => MiscUtils.FeetToMillimeters(_height); set => _height = value; }
        public string FullExportName { get => _fullExportName; set => _fullExportName = value; }
        public ElementId Id { get => _id; set => _id = value; }
        public string PrintSettingsName { get => this._printSetting != null ? this._printSetting.Name : string.Empty; }
        public bool ValidScaleBar
        {
            get { return this.RevitScaleWithoutFormatting() == this._scaleBarScale.Trim(); }
        }
        public string ExportDirectory { get; set; }
        public ViewSheet Sheet { get => _sheet; }
        public bool ForceDate
        {
            get => _forceDate;
            set
            {
                _forceDate = value;
                this.SetExportName();
            }
        }

        public string RevitScaleWithoutFormatting()
        {
            string result = this._scale.Trim();
            int i = 0;
            if (result.Contains(":"))
            {
                i = result.IndexOf(':');
            }
            else return "0";
            return string.IsNullOrEmpty(result.Trim()) ? "0" : result.Substring(i + 2).Trim();
        }

        private void SetExportName()
        {
            if (_forceDate)
            {
                this._sheetRevision = MiscUtils.GetDateString;
            }
            else
            {
                this._sheetRevision = this._sheet.get_Parameter(BuiltInParameter.SHEET_CURRENT_REVISION).AsString();
            }

            if (this._sheetRevision.Length < 1)
            {
                this._sheetRevision = MiscUtils.GetDateString;
            }
            this._fullExportName = this.PopulateSegmentedFileName();
        }

        private string PopulateSegmentedFileName()
        {
            return PostExportHookCommand.FormatConfigurationString(this, this._segmentedFileName.NameFormat, string.Empty);
        }

        #endregion
        public override string ToString()
        {
            return string.Format(
                "Sheet information: " + Environment.NewLine +
                "    SheetRevisionDateTime={0}," + Environment.NewLine +
                "    Doc={1}," + Environment.NewLine +
                "    Id={2}," + Environment.NewLine +
                "    PrintSetting={3}," + Environment.NewLine +
                "    SegmentedFileName={4}," + Environment.NewLine +
                "    Sheet={5}," + Environment.NewLine +
                "    ForceDate={6}," + Environment.NewLine +
                "    Verified={7}," + Environment.NewLine +
                "    Height={8}," + Environment.NewLine +
                "    Width={9}," + Environment.NewLine +
                "    FullExportName={10}," + Environment.NewLine +
                "    PageSize={11}," + Environment.NewLine +
                "    ProjectNumber={12}," + Environment.NewLine +
                "    Scale={13}," + Environment.NewLine +
                "    ScaleBarScale={14}," + Environment.NewLine +
                "    SheetDescription={15}," + Environment.NewLine +
                "    SheetNumber={16}," + Environment.NewLine +
                "    SheetRevision={17}," + Environment.NewLine +
                "    SheetRevisionDate={18}," + Environment.NewLine +
                "    SheetRevisionDescription={19}," + Environment.NewLine +
                "    ExportDir={20}",
                this._sheetRevisionDateTime,
                this._doc.PathName,
                this.Id,
            this._printSetting,
            this._segmentedFileName,
            this._sheet,
            this._forceDate,
            this._verified,
            this.Height,
            this.Width,
            this.FullExportName,
            this._pageSize,
            this._projectNumber,
            this._scale,
            this._scaleBarScale,
            this._sheetDescription,
            this._sheetNumber,
            this._sheetRevision,
            this._sheetRevisionDate,
            this._sheetRevisionDescription,
            this.ExportDirectory);
        }
    }
}
