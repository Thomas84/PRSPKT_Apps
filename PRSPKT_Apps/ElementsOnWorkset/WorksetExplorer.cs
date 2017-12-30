// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using PRSPKT_Apps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ElementsOnWorkset
{
    [Transaction(TransactionMode.Manual)]
    public class WorksetExplorer : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument UIdoc = commandData.Application.ActiveUIDocument;
            Document doc = UIdoc.Document;

            using (Transaction t = new Transaction(doc))
            {
                try
                {
                    MyWorksetExplorer(UIdoc);
                    return Result.Succeeded;
                }
                catch (OperationCanceledException cancelled)
                {
                    message = cancelled.Message;
                    if (t.HasStarted())
                    {
                        t.RollBack();
                    }
                    return Result.Failed;
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

        private void MyWorksetExplorer(UIDocument UIdoc)
        {
            Document _doc = UIdoc.Document;

            // Выборка всех элементов в проекте
            IList<Element> allElements = new FilteredElementCollector(_doc).WhereElementIsNotElementType().ToElements();

            if (_doc.IsWorkshared)
            {
                WorksetTable worksetTable = _doc.GetWorksetTable();
                IList<Workset> worksets = new FilteredWorksetCollector(_doc).OfKind(WorksetKind.UserWorkset).ToWorksets();
                List<List<Element>> allElsEls = new List<List<Element>>(worksets.Count);
                foreach (Workset ws in worksets)
                {
                    allElsEls.Add(new List<Element>());
                }

                foreach (Element element in allElements)
                {
                    Workset workset = worksetTable.GetWorkset(element.WorksetId); // Get's the Workset Table of the document. Then return the workset from the input WorksetId
                    if (element.Category != null &&
                        (element.Category.Id.IntegerValue != (int)BuiltInCategory.OST_Grids &&
                        element.Category.Id.IntegerValue != (int)BuiltInCategory.OST_Levels &&
                        element.Category.Id.IntegerValue != (int)BuiltInCategory.OST_PreviewLegendComponents))
                    {
                        Parameter param = element.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM);
                        if (param != null && !param.IsReadOnly)
                        {
                            bool flag = false;
                            int index = -1;
                            foreach (Workset ws in worksets)
                            {
                                ++index;
                                if (Equals(ws.Id, workset.Id))
                                {
                                    flag = true;
                                    break;
                                }
                            }
                            if (flag)
                            {
                                if (index != -1)
                                {
                                    allElsEls[index].Add(element);
                                }

                            }
                        }
                    }
                }


                var infoWorksetForm = new PRSPKT_Apps.ElementsOnWorkset.WorksetExplorerForm(allElsEls, _doc, ((IEnumerable<Workset>)worksets).ToArray());
                if (DialogResult.OK == infoWorksetForm.ShowDialog())
                {
                    List<Element> selectedElements = infoWorksetForm.GetSelectedElements();
                    UIdoc.Selection.SetElementIds(selectedElements.Select(q => q.Id).ToList());
                }
            }
            else
            {
                TaskDialog.Show("Ошибка", "Проект не переведён в режим совместной работы");
            }
        }
    }
}
