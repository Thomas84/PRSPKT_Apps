// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using PRSPKT_Apps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils
{
    [Transaction(TransactionMode.Manual)]
    public class CurveTotalLength : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get application and document objects
            UIDocument UIdoc = commandData.Application.ActiveUIDocument;
            Document doc = UIdoc.Document;
            try
            {
                IList<Element> pickedRef = null;
                List<ElementId> selectedElements = UIdoc.Selection.GetElementIds().ToList();
                if (selectedElements.Count == 0)
                {
                    pickedRef = UIdoc.Selection.PickObjects(ObjectType.Element, new LineSelectionFilter(), "Выберите линии детализации")
                    .Select(q => doc.GetElement(q.ElementId))
                    .ToList();
                }
                else pickedRef = selectedElements.Select(q => doc.GetElement(q)).ToList();

                if (pickedRef.Count == 0)
                {
                    return Result.Failed;
                }

                // Measure their total length
                List<double> lengthList = new List<double>();
                foreach (Element e in pickedRef)
                {
                    if (e is DetailLine line)
                    {
                        if (line != null)
                        {
                            lengthList.Add(line.GeometryCurve.Length);
                        }
                    }
                    if (e is ModelLine m_line)
                    {
                        lengthList.Add(m_line.GeometryCurve.Length);
                    }

                }

                string lengthMm = Tools.RealString(Math.Round(lengthList.Sum() * 304.8, 2)) + " мм";
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Общая длина:");
                sb.AppendLine(lengthMm);

                // Return a message window that displays total length to user
                TaskDialog.Show("Длина", sb.ToString());
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
