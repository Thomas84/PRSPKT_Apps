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
    public class WallsTotalArea : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument UIdoc = commandData.Application.ActiveUIDocument;
            Document doc = UIdoc.Document;

            using (Transaction t = new Transaction(doc))
            {
                try
                {
                    IList<Element> pickedRef = null;
                    List<ElementId> selectedElements = UIdoc.Selection.GetElementIds().ToList();
                    if (selectedElements.Count == 0)
                    {
                        pickedRef = UIdoc.Selection.PickObjects(ObjectType.Element, new WallsSelectionFilter(), "Выберите стены")
                        .Select(q => doc.GetElement(q.ElementId))
                        .ToList();
                    }
                    else pickedRef = selectedElements.Select(q => doc.GetElement(q)).ToList();

                    if (pickedRef.Count == 0)
                    {
                        return Result.Failed;
                    }

                    // Measure their total length
                    List<double> areaList = new List<double>();

                    foreach (Element wall in pickedRef)
                    {
                        if (wall is Wall)
                        {
                            Parameter param = wall.get_Parameter(BuiltInParameter.HOST_AREA_COMPUTED);
                            if (param != null && StorageType.Double == param.StorageType)
                            {
                                areaList.Add(param.AsDouble() * 0.092903);
                            }
                            else areaList.Add(0.0);
                        }

                    }

                    string areaM2 = Tools.RealString(Math.Round(areaList.Sum(), 2)) + " м2";
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("Общая площадь:");
                    sb.AppendLine(areaM2);

                    // Return a message window that displays total length to user
                    TaskDialog.Show("Площадь стен", sb.ToString());
                    return Result.Succeeded;
                }

                catch (OperationCanceledException exceptionCancelled)
                {
                    message = exceptionCancelled.Message;
                    if (t.HasStarted())
                    {
                        t.RollBack();
                    }
                    return Result.Cancelled;
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
