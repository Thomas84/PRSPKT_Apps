using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace PRSPKT_Apps.RevisionItems
{
    public class Command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            if (commandData == null)
            {
                return Result.Failed;
            }

            Document doc = commandData.Application.ActiveUIDocument.Document;
            RevisionUserControl form = new RevisionUserControl(doc);

            form.ShowDialog();

            return Result.Succeeded;
        }
    }
}
