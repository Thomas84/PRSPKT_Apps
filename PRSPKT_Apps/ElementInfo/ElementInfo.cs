using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRSPKT_Apps
{
    [Transaction(TransactionMode.Manual)]
    public class ElementInfo : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument UIdoc = commandData.Application.ActiveUIDocument;
            Document doc = UIdoc.Document;

            var selection = doc.GetElement(UIdoc.Selection.GetElementIds().FirstOrDefault());

            try
            {
                if (doc.IsWorkshared)
                {
                    var wti = WorksharingUtils.GetWorksharingTooltipInfo(doc, selection.Id);
                    TaskDialog.Show(Tools.GetResourceManager("element_info"), string.Format(
                        "Создал: {0}\n" +
                        "Текущий владелец: {1}\n" +
                        "Последний раз изменен: {2}",
                        wti.Creator,
                        wti.Owner,
                        wti.LastChangedBy));
                    return Result.Succeeded;
                }
                else
                {
                    TaskDialog.Show(Tools.GetResourceManager("element_info"), "Модель не совместная");
                    return Result.Failed;
                }
            }
            catch (Exception)
            {
                return Result.Cancelled;
            }

        }
    }
}
