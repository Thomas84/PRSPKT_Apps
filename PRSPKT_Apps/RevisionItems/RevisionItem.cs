using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRSPKT_Apps.RevisionItems
{
    public class RevisionItem
    {
        private string _description;
        private string _date;
        private bool _isIssued;
        private int _sequence;

        public RevisionItem(Document doc, RevisionCloud revisionCloud)
        {
            if (revisionCloud == null)
            {
                throw new ArgumentNullException("revisionCloud");
            }
            if (doc == null)
            {
                throw new ArgumentNullException("doc");
            }
            var revision = doc.GetElement(revisionCloud.RevisionId);
            Init(revision);
        }

        public RevisionItem(Revision revision)
        {
            Init(revision);
        }

        private void Init(Element revision)
        {
            this._description = revision.get_Parameter(BuiltInParameter.PROJECT_REVISION_REVISION_DESCRIPTION).AsString();
            this._date = revision.get_Parameter(BuiltInParameter.PROJECT_REVISION_REVISION_DATE).AsString();
            this._isIssued = revision.get_Parameter(BuiltInParameter.PROJECT_REVISION_REVISION_ISSUED).AsInteger() == 1;
            this._sequence = revision.get_Parameter(BuiltInParameter.PROJECT_REVISION_SEQUENCE_NUM).AsInteger();
        }

        public bool Export { get; set; }

        public string Description
        {
            get { return this._description; }
        }

        public string Date
        {
            get { return this._date; }
        }

        public int Sequence
        {
            get { return this._sequence; }
        }

        public bool Issued
        {
            get { return this._isIssued; }
        }
    }
}
