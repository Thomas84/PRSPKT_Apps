// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace Utils
{
    public class GridWallSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem.Name.Contains("Отделка") || elem.Name.Contains("Шт") || elem.Name.Contains("ОС-"))
            {
                return false;
            }
            else
            {
                return elem is Wall ||
                        elem.Category.Id.IntegerValue.Equals((int)BuiltInCategory.OST_Grids) ||
                        elem.Category.Id.IntegerValue.Equals((int)BuiltInCategory.OST_WallsDefault) ||
                        elem.Category.Id.IntegerValue.Equals((int)BuiltInCategory.OST_WallsInsulation) ||
                        elem.Category.Id.IntegerValue.Equals((int)BuiltInCategory.OST_WallsStructure);
            }
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
