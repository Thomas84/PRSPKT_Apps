using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using PRSPKT_Apps;
using Autodesk.Revit.Attributes;

namespace objectsOnWorkset
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument UIdoc = commandData.Application.ActiveUIDocument;
            Document doc = UIdoc.Document;

            using (Transaction t = new Transaction(doc))
            {
                try
                {
                    var worksetList = new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset);
                    if (doc.IsWorkshared)
                    {
                        StringBuilder sb = new StringBuilder();

                        foreach (var ws in worksetList)
                        {
                            var elementWorksetFilter = new ElementWorksetFilter(ws.Id, false);

                            var elementCollector = new FilteredElementCollector(doc)
                                .WherePasses(elementWorksetFilter).ToElements();
                            sb.AppendLine(String.Format("WORKSET: {0} ID: {1} COUNT: {2}", ws.Name, ws.Id, elementCollector.Count));
                            List<String> elementsArray = new List<String>();
                            foreach (Element e in elementCollector)
                            {
                                elementsArray.Add(e.Id.ToString());
                            }
                            sb.AppendLine(string.Join(", ", elementsArray));
                            sb.AppendLine(System.Environment.NewLine + System.Environment.NewLine);
                        }
                        TaskDialog.Show("Workset", sb.ToString());
                    }
                    else
                    {
                        TaskDialog.Show("Error", "Model is not workshared");
                    }
                    return Result.Succeeded;
                }
                catch (ErrorMessageException errorException)
                {
                    message = errorException.Message;
                    if (t.HasStarted())
                    {
                        t.RollBack();
                    }
                    return Result.Failed;
                }
                catch (Exception ex)
                {
                    message = "Неожиданная ошибка: " + ex.Message;
                    if (t.HasStarted())
                    {
                        t.RollBack();
                    }
                    return Result.Failed;
                }
            }
        }
    }
}
