// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;

namespace Utils
{
    [Transaction(TransactionMode.Manual)]
    public class DeleteImportLines : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            IList<Element> linePatternList = new FilteredElementCollector(doc)
                .WherePasses(new ElementClassFilter(typeof(LinePatternElement)))
                .Where(q => q.Name.StartsWith("IMPORT")).ToList(); ;

            string msg = string.Empty;
            if (linePatternList.Count > 0)
            {
                using (var t = new Transaction(doc, "Зачистка импорт линий"))
                {
                    t.Start();
                    foreach (var linePatterElement in linePatternList)
                    {
                        if (linePatterElement is LinePatternElement)
                        {
                            msg += linePatterElement.Name + "\n";
                            doc.Delete(linePatterElement.Id);
                        }

                    }
                    t.Commit();
                    TaskDialog.Show("Line", "Удаленные типы линий: \n" + msg, TaskDialogCommonButtons.Ok);

                }
            }
            else
            {
                TaskDialog.Show("Line", "Такие типы линий не найдены" + msg, TaskDialogCommonButtons.Ok);
            }

            return Result.Succeeded;
        }
    }
}
