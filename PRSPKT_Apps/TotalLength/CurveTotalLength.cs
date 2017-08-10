using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TotalLength
{
    [Transaction(TransactionMode.Manual)]
    public class CurveTotalLength : IExternalCommand
    {
        public class DetailLineFilter : ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                return (elem.Category.Id.IntegerValue.Equals( (int)BuiltInCategory.OST_Lines));
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                return false;
            }
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get application and document objects
            UIApplication uiApp = commandData.Application;
            try
            {
                IList<Element> pickedRef = null;
                Selection sel = uiApp.ActiveUIDocument.Selection;
                DetailLineFilter selFilter = new DetailLineFilter();
                pickedRef = sel.PickElementsByRectangle(selFilter, "Выберите линии");

                // Measure their total length
                List<double> lengthList = new List<double>();
                foreach (Element e in pickedRef)
                {
                    DetailLine line = (DetailLine)e;
                    if (line != null)
                    {
                        lengthList.Add(line.GeometryCurve.Length);
                    }
                }

                string lengthMm = Math.Round(lengthList.Sum() * 304.8, 2).ToString() + " мм";
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Общая длина:");
                sb.AppendLine(lengthMm);

                // Return a message window that displays total length to user
                TaskDialog.Show("Line Length", sb.ToString());
                // Assuming that everything went right return Result.Succeeded
                return Result.Succeeded;


            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }
    }
}
