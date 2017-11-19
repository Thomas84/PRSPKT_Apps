#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Diagnostics;
#endregion

namespace PRSPKT_Apps
{
    [Transaction(TransactionMode.Manual)]
    public class CommandQ : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            // Access current Selection

            Selection sel = uidoc.Selection;

            // Retrieve elements from database
            FilteredElementCollector col
                = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.INVALID)
                .OfClass(typeof(Wall));

            // Filtered element collector is iterable

            foreach (Element e in col)
            {
                Debug.Print(e.Name);
            }

            // Modify document within a transaction

            using (Transaction t = new Transaction(doc))
            {
                t.Start("Transaction Name");
                t.Commit();
            }

            return Result.Succeeded;
        }
    }
}
