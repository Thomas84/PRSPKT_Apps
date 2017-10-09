using Autodesk.Revit.DB;

namespace PRSPKT_Apps.ExportManager
{
    public class ViewSheetSetCombo
    {
        private string _customName;

        public ViewSheetSetCombo(ViewSheetSet viewSheetSet)
        {
            this.ViewSheetSet = viewSheetSet;
            this._customName = this.ViewSheetSet.Name;
        }

        public ViewSheetSetCombo(string name)
        {
            this.ViewSheetSet = null;
            this._customName = name;
        }

        public ViewSheetSet ViewSheetSet { get; set; }
        public string CustomName { get => _customName; }
        public override string ToString()
        {
            return this._customName;
        }
    }
}
