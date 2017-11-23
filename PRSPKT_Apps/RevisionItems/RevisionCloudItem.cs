using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevisionItems
{
    public class RevisionCloudItem : RevisionItem
    {
        private string _sheetNumber;
        private string _sheetName;
        private string _revision;
        private string _mark;
        private string _comments;
        private string _viewName;
        private ElementId _id;
        private RevisionCloud cloud;

        public RevisionCloudItem(Document doc, RevisionCloud revisionCloud) : base(doc, revisionCloud)
        {
            this._mark = revisionCloud.get_Parameter(BuiltInParameter.ALL_MODEL_MARK).AsString();
            this._comments = revisionCloud.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString();
            this._id = revisionCloud.Id;
            this._revision = string.Empty;
            this.cloud = revisionCloud;
            this._viewName = GetHostViewName(doc);
            UpdateSheetNameAndNumberStrings(doc);
        }

        public string SheetNumber { get { return this._sheetNumber; } }
        public string SheetName { get { return this._sheetName; } }
        public string ViewName { get { return this._viewName; } }
        public ElementId Id { get { return this._id; } }
        public string Revision { get { return this._revision; } }
        public string Mark { get { return this._mark; } }
        public string Comments { get { return this._comments; } }

        public void SetCloudId(ElementId revisionId)
        {
            cloud.RevisionId = revisionId;
        }

        private void UpdateSheetNameAndNumberStrings(Document doc)
        {
            this._sheetName = "-";
            this._sheetNumber = "-";
            if (cloud.GetSheetIds().Count == 1)
            {
                ElementId id2 = cloud.GetSheetIds().ToList().First<ElementId>();
                if (id2 != null)
                {
                    Element element = doc.GetElement(id2);
                    ViewSheet viewSheet = (ViewSheet)element;
                    if (viewSheet != null)
                    {
                        this._sheetNumber = viewSheet.SheetNumber;
                        this._revision = viewSheet.get_Parameter(BuiltInParameter.SHEET_CURRENT_REVISION).AsString();
                        this._sheetName = viewSheet.Name;
                    }
                }
            }
            if (cloud.GetSheetIds().Count > 1)
            {
                this._sheetName = "Multiple";
                this._sheetNumber = "Multiple";
            }
        }

        private string GetHostViewName(Document doc)
        {
            return doc.GetElement(cloud.OwnerViewId).Name;
        }
    }
}
