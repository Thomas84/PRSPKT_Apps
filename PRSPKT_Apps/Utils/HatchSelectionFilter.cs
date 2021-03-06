﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace Utils
{
    public class HatchSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            return elem.Category.Id.IntegerValue.Equals((int)BuiltInCategory.OST_FilledRegion) ||
               elem.Category.Id.IntegerValue.Equals((int)BuiltInCategory.OST_FillPatterns) ||
               elem.Category.Id.IntegerValue.Equals((int)BuiltInCategory.OST_MaskingRegion) ||
               elem.Category.Id.IntegerValue.Equals((int)BuiltInCategory.OST_DetailComponents);
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
