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

            int num1 = -2000576; // OST_PreviewLegendComponents
            int num2 = -2000220; // OST_Grids
            int num3 = -2000240; // OST_Levels

            // Выборка всех элементов в проекте
            IList<Element> elements1 = new FilteredElementCollector(_doc).WhereElementIsNotElementType().ToElements();
            if (_doc.IsWorkshared)
            {
                WorksetTable worksetTable = _doc.GetWorksetTable();
                IList<Workset> worksets = new FilteredWorksetCollector(_doc).OfKind(WorksetKind.UserWorkset).ToWorksets();
                List<List<Element>> allElsEls = new List<List<Element>>(worksets.Count);
                foreach (Workset ws in worksets)
                {
                    allElsEls.Add(new List<Element>());
                }

                foreach (Element el in elements1)
                {
                    Workset workset = worksetTable.GetWorkset(el.WorksetId); // Get's the Workset Table of the document. Then return the workset from the input WorksetId
                    if (el.Category != null && (num2 != el.Category.Id.IntegerValue && num3 != el.Category.Id.IntegerValue && num1 != el.Category.Id.IntegerValue))
                    {
                        Parameter param = el.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM);
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
                                allElsEls[index].Add(el);
                            }
                        }
                    }
                }

                // Load the selection WorksetExplorer form
                
                var infoWWE = new PRSPKT_Apps.ElementsOnWorkset.WorksetExplorerForm(allElsEls, _doc, ((IEnumerable<Workset>)worksets).ToArray());
                if (DialogResult.OK == infoWWE.ShowDialog())
                {
                    List<Element> selectedElements = infoWWE.GetSelectedElements();
                    UIdoc.Selection.SetElementIds(selectedElements.Select(q => q.Id).ToList());
                }
            }
            else
            {
                TaskDialog.Show("Message", "Cannot analyze worksets because worksharing hasn't been enabled for this project");
            }
        }
    }
}
