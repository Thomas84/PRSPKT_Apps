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
            //TODO:
            // create an User Interface:
            // 1. Name of the Worksets in the entire project
            // 2. Category in each workset
            // 3. Types of elements in category
            // There are two buttons - Cancel and Select Elements

            using (Transaction t = new Transaction(doc))
            {
                try
                {
                    int num1 = -2000576;
                    int num2 = -2000220;
                    int num3 = -2000240;
                    IList<Element> elements1 = new FilteredElementCollector(doc).WhereElementIsNotElementType().ToElements();
                    if (doc.IsWorkshared)
                    {
                        WorksetTable worksetTable = doc.GetWorksetTable();
                        IList<Workset> worksets = new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset).ToWorksets();
                        List<List<Element>> allElsEls = new List<List<Element>>(worksets.Count);
                        foreach (Workset ws in worksets)
                        {
                            allElsEls.Add(new List<Element>());
                        }


                        foreach (Element el in elements1)
                        {
                            Workset workset = worksetTable.GetWorkset(el.WorksetId);
                            if (el.Category != null && (num2 == el.Category.Id.IntegerValue || num3 == el.Category.Id.IntegerValue))
                            {
                                Parameter param = el.get_Parameter(BuiltInParameter.ELEM_PARTITION_PARAM);
                                if (param != null && !param.IsReadOnly)
                                {
                                    bool flag = false;
                                    int index = -1;
                                    foreach (Workset ws in worksets)
                                    {
                                        ++index;
                                        if (WorksetId.Equals(((WorksetPreview)ws).Id, ((WorksetPreview)workset).Id)))
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
                        InfoWindowWorksetExplorer new InfoWindowWorksetExplorer(allElsEls, doc, Workset worksets);
                        if (DialogResult.OK == InfoWindowWorksetExplorer.ShowDialog())
                        {
                            List<Element> selectedElements = InfoWindowWorksetExplorer.getSelectedElements();
                            foreach (Element current in selectedElements)
                            {
                                doc.SetSelection
                            }
                        }
                    }
                    else
                    {
                        TaskDialog.Show("Message", "Cannot analyze worksets because worksharing hasn't been enabled for this project");
                    }
                    /*
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
                    return Result.Succeeded;*/
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
