#region Namespaces
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using PRSPKT_Apps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
#endregion

namespace PrintMe
{
    [Transaction(TransactionMode.Manual)]
    public class PrintMe : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument UIdoc = commandData.Application.ActiveUIDocument;
            Document doc = UIdoc.Document;

            using (Transaction t = new Transaction(doc))
            {
                try
                {
                    PrintAsIs(UIdoc, t);
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

        private void PrintAsIs(UIDocument UIdoc, Transaction t)
        {
            Document _doc = UIdoc.Document;
            List<Element> titleBlockList = new FilteredElementCollector(_doc)
                .OfCategory(BuiltInCategory.OST_TitleBlocks).WhereElementIsNotElementType().ToList();

            List<View> sheetList = new FilteredElementCollector(_doc).OfCategory(BuiltInCategory.OST_Sheets).ToList().ConvertAll(q => { return _doc.GetElement(q.Id) as View; });

            SortedList<string, ViewSet> viewSheetSetsList = new SortedList<string, ViewSet>(); // коллекция ключ (строка) / значение (ViewSet)

            foreach (Element element in titleBlockList)
            {
                Parameter s_number = element.get_Parameter(BuiltInParameter.SHEET_NUMBER);
                View sheet = sheetList.Where(v => v.get_Parameter(BuiltInParameter.SHEET_NUMBER).AsString() == s_number.AsString()).First();
                Parameter s_height = element.get_Parameter(BuiltInParameter.SHEET_HEIGHT);
                Parameter s_width = element.get_Parameter(BuiltInParameter.SHEET_WIDTH);

                string s_complect = "";

                // получаем "комплект" из листа с тем же номером, что у titleBlock
                foreach (Element sh in sheetList)
                {
                    Parameter sheet_number = sh.get_Parameter(BuiltInParameter.SHEET_NUMBER);
                    if (sheet_number.AsString() == s_number.AsString())
                    {
                        s_complect = sh.GetParameters("Раздел проекта")[0].AsString();
                        break;
                    }
                }
                int d_height = (int)Math.Round(304.799472 * s_height.AsDouble());
                int d_width = (int)Math.Round(304.799472 * s_width.AsDouble());
                string s_format = string.Format("Раздел {0}: формат {1}", s_complect, GetSheetFormat(d_height.ToString(), d_width.ToString()));

                if (viewSheetSetsList.Keys.Contains(s_format))
                {
                    ViewSet viewSet = viewSheetSetsList.Values.ElementAt(viewSheetSetsList.Keys.IndexOf(s_format));
                    viewSet.Insert(sheet);
                }
                else
                {
                    ViewSet viewSet = new ViewSet();
                    viewSet.Insert(sheet);
                    viewSheetSetsList.Add(s_format, viewSet);
                }
            }

            using (TransactionGroup tg = new TransactionGroup(_doc))
            {
                tg.Start("Sorting ViewSheets by format");
                using (Transaction tx1 = new Transaction(_doc))
                {
                    tx1.Start("Deleting existing ViewSheet Sets");
                    List<ElementId> allViewSheetSetsIds = new FilteredElementCollector(_doc).OfClass(typeof(ViewSheetSet))
                        .ToElementIds().ToList();
                    _doc.Delete(allViewSheetSetsIds);
                    tx1.Commit();
                }
                using (Transaction tx2 = new Transaction(_doc))
                {
                    tx2.Start("Creating new ViewSheet Sets");
                    PrintManager printManager = _doc.PrintManager;
                    printManager.PrintRange = PrintRange.Select;
                    ViewSheetSetting viewSheetSetting = printManager.ViewSheetSetting;
                    foreach (KeyValuePair<string, ViewSet> keyValuePair in viewSheetSetsList)
                    {
                        viewSheetSetting.CurrentViewSheetSet.Views = keyValuePair.Value;
                        viewSheetSetting.SaveAs(keyValuePair.Key);
                    }
                    tx2.Commit();
                }
                tg.Assimilate();
            }
        }

        private string GetSheetFormat(string height, string width)
        {
            string output;
            switch (width + "x" + height)
            {
                case "1680x1188":output = "A0x2A"; break;
                case "1188x840": output = "A0А"; break;
                case "2520x1188": output = "A0x3А"; break;
                case "840x594": output = "A1А"; break;
                case "1782x840": output = "A1x3А"; break;
                case "2376x840": output = "A1x4А"; break;
                case "2970x840": output = "A1x5А"; break;
                case "594x420": output = "A2А"; break;
                case "794x420": output = "A2A+"; break;
                case "1260x594": output = "A2x3А"; break;
                case "1680x594": output = "A2x4А"; break;
                case "2100x594": output = "A2x5А"; break;
                case "420x297": output = "A3А"; break;
                case "891x420": output = "A3x3А"; break;
                case "1188x420": output = "A3x4А"; break;
                case "1485x420": output = "A3x5А"; break;
                case "1782x420": output = "A3x6А"; break;
                case "2079x420": output = "A3x7А"; break;
                case "297x210": output = "A4А"; break;
                case "630x297": output = "A4x3А"; break;
                case "840x297": output = "A4x4А"; break;
                case "1050x297": output = "A4x5А"; break;
                case "1260x297": output = "A4x6А"; break;
                case "1188x1680": output = "A0x2K"; break;
                case "840x1188": output = "A0K"; break;
                case "1188x2520": output = "A0x3K"; break;
                case "594x840": output = "A1K"; break;
                case "840x1782": output = "A1x3K"; break;
                case "840x2376": output = "A1x4K"; break;
                case "840x2970": output = "A1x5K"; break;
                case "420x594": output = "A2K"; break;
                case "594x1260": output = "A2x3K"; break;
                case "594x1680": output = "A2x4K"; break;
                case "594x2100": output = "A2x5K"; break;
                case "297x420": output = "A3K"; break;
                case "420x891": output = "A3x3K"; break;
                case "420x1188": output = "A3x4K"; break;
                case "420x1485": output = "A3x5K"; break;
                case "420x1782": output = "A3x6K"; break;
                case "420x2079": output = "A3x7K"; break;
                case "210x297": output = "A4K"; break;
                case "297x630": output = "A4x3K"; break;
                case "297x840": output = "A4x4K"; break;
                case "297x1050": output = "A4x5K"; break;
                case "297x1260": output = "A4x6K"; break;
                default: output = "хз.."; break;
            }
            return output;
        }
    }
}
