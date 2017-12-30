// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace PRSPKT_Apps.FloorEdit
{
    public class FloorSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            return (elem.Category.Id.IntegerValue.Equals((int)BuiltInCategory.OST_Floors));
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
