// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

/* UserView.cs
 * prspkt.ru
 * © PRSPKT Architects, 2018
 *
 * This updater is used to create an updater capable of reacting
 * to changes in the Revit model.
 */
#region Namespaces

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using PRSPKT_Apps.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ArgumentException = Autodesk.Revit.Exceptions.ArgumentException;

#endregion

namespace UserView
{

    public class UserView
    {
        public static List<View> Create(UIDocument uiDoc, View srcView, string pName="", string desc="")
        {
            var doc = uiDoc.Document;
            if (srcView == null || doc == null)
            {
                return null;
            }

            if (srcView.ViewType == ViewType.DrawingSheet)
            {

                return Create(uiDoc, srcView as ViewSheet);
            }

            if (!ValidViewType(srcView.ViewType)) return null;
            var result = new List<View> { CreateView(uiDoc, srcView, pName, desc) };
            return result;
        }

        public static void ShowSummaryDialog(List<View> srcViews)
        {
            using (var t = new TaskDialog("Создать вид"))
            {
                var msg = String.Empty;
                if (srcViews == null)
                {
                    msg = "Невозможно создать виды\n"
                          + "\tИспользуйте следующие виды:\n\n"
                          + "\t\tПланы, Фасады, Разрезы, \n"
                          + "\t\tПланы потолков, Планы площадей,\n "
                          + "\t\t3D";
                }
                else
                {
                    foreach (View view in srcViews)
                    {
                        msg += view.Name + "\n";
                    }
                }
                t.MainIcon = TaskDialogIcon.TaskDialogIconNone;
                t.MainInstruction = "Сведения о созданных(ом) видах(е): ";
                t.MainContent = msg;
                t.Show();
            }
        }

        //        public static void ShowSummaryDialog(string msg)
        //        {
        //            using (var t = new TaskDialog("Создать вид"))
        //            {
        //                t.MainIcon = TaskDialogIcon.TaskDialogIconNone;
        //                t.MainInstruction = "Сведения о созданных видах: ";
        //                t.MainContent = msg;
        //                t.Show();
        //            }
        //        }

        public static bool ValidViewType(ViewType viewType)
        {
            switch (viewType)
            {
                case ViewType.FloorPlan:
                case ViewType.Elevation:
                case ViewType.CeilingPlan:
                case ViewType.Section:
                case ViewType.AreaPlan:
                case ViewType.ThreeD:
                    return true;
            }
            return false;
        }

        public static View CreateView(UIDocument uiDoc, View srcView, string pName="", string desc="")
        {
            var doc = uiDoc.Document;
            if (srcView == null) return null;
            var view = ActiveUIView(uiDoc, srcView);
            var targetViewId = srcView.Duplicate(ViewDuplicateOption.WithDetailing);
            if (!(doc.GetElement(targetViewId) is View newView)) return null;
            if (string.IsNullOrEmpty(pName) || string.IsNullOrEmpty(desc))
            {
                newView.Name = GetNewViewName(doc, srcView);
            }
            else newView.Name = GetNewViewName(doc, srcView, pName, desc);
            newView.ViewTemplateId = ElementId.InvalidElementId;
            ApplyBBoxToView(
                newView.ViewType == ViewType.Section
                    ? SectionViewExtentsBoundingBox(view)
                    : ViewExtentsBoundingBox(view), newView);
            return (View)TryGetParameter(newView, "Раздел проекта");
        }

        public static Element TryGetParameter(Element element, string p)
        {
            var parameters = element.GetParameters(p);
            if (parameters.Count < 1)
            {
                return element;
            }
            var parameter = parameters[0];
            if (parameter == null)
            {
                return element;
            }

            if (parameter.IsReadOnly)
            {
                TaskDialog.Show("Ошибка", "Только для чтения");
                return null;
            }
            else
            {
                if (parameter.Set("ЗАДАНИЕ")) return element;
                TaskDialog.Show("Ошибка", "Ошибка при задани Раздел проекта");
                return null;
            }
        }

        public static string GetNewViewName(Document doc, View srcView)
        {
            if (doc == null || srcView == null)
            {
                return string.Empty;
            }

            var name = srcView.Name;
            var prefix = srcView.ViewType != ViewType.ThreeD
                ? "(З)_"
                : "(Персп.)_";
            name = name.Replace(@"{", String.Empty).Replace(@"}", string.Empty);
            name = $"{prefix} от {MiscUtils.GetDateString} - {name}";
            return MiscUtils.ViewNameIsAvailable(doc, name)
                ? name
                : MiscUtils.GetNiceViewName(doc, name);
        }

        public static string GetNewViewName(Document doc, View srcView, string pName, string desc)
        {
            if (doc == null || srcView == null)
            {
                return string.Empty;
            }

            var name = desc;
            var prefix = srcView.ViewType != ViewType.ThreeD
                ? $"(З)_{pName}_"
                : "(Персп.)_";
            name = name.Replace(@"{", string.Empty).Replace(@"}", string.Empty);
            name = $"{prefix}{MiscUtils.GetDateString} - {name}";
            return MiscUtils.ViewNameIsAvailable(doc, name)
                ? name
                : MiscUtils.GetNiceViewName(doc, name);
        }

        public static List<View> Create(UIDocument uiDoc, ViewSheet viewSheet)
        {
            var doc = uiDoc.Document;
            return (from id in viewSheet.GetAllPlacedViews()
                    select doc.GetElement(id) as View
                into v
                    where ValidViewType(v.ViewType)
                    select CreateView(uiDoc, v)).ToList();
        }

        public static BoundingBoxXYZ ViewExtentsBoundingBox(UIView view)
        {
            if (view == null) return new BoundingBoxXYZ();
            var result = new BoundingBoxXYZ();
            var min = new XYZ(view.GetZoomCorners()[0].X, view.GetZoomCorners()[0].Y, view.GetZoomCorners()[0].Z - 4);
            var max = new XYZ(view.GetZoomCorners()[1].X, view.GetZoomCorners()[1].Y, view.GetZoomCorners()[1].Z + 4);
            result.set_Bounds(0, min);
            result.set_Bounds(1, max);
            return result;
        }

        public static BoundingBoxXYZ SectionViewExtentsBoundingBox(UIView view)
        {
            if (view == null) return new BoundingBoxXYZ();
            var result = new BoundingBoxXYZ();
            try
            {
                var min = new XYZ(view.GetZoomCorners()[0].X, view.GetZoomCorners()[0].Y, view.GetZoomCorners()[0].Z - 4);
                var max = new XYZ(view.GetZoomCorners()[1].X, view.GetZoomCorners()[1].Y, view.GetZoomCorners()[1].Z + 4);
                result.set_Bounds(0, min);
                result.set_Bounds(1, max);
            }
            catch (ArgumentException exception)
            {
                Debug.WriteLine(exception.Message);
                result.Dispose();
                return null;
            }

            return result;
        }

        public static void ApplySectionBoxToView(BoundingBoxXYZ boundingBox, View3D view3D)
        {
            if (boundingBox != null || view3D != null)
            {
                view3D?.SetSectionBox(boundingBox);
            }
        }

        public static void ApplyBBoxToView(BoundingBoxXYZ bbox, View view)
        {
            if (bbox == null || view == null) return;
            view.CropBoxActive = true;
            view.CropBoxVisible = false;
            view.CropBox = bbox;

        }

        public static List<ViewFamilyType> Get3DViewFamilyTypes(Document doc)
        {
            var viewFamilyTypes = new List<ViewFamilyType>();
            using (var collector = new FilteredElementCollector(doc))
            {
                collector.OfClass(typeof(ViewFamilyType));
                foreach (var element in collector)
                {
                    var viewFamilyType = (ViewFamilyType)element;
                    if (viewFamilyType.ViewFamily == ViewFamily.ThreeDimensional)
                    {
                        viewFamilyTypes.Add(viewFamilyType);
                    }
                }
            }

            return viewFamilyTypes;
        }

        /// <summary>
        /// Get Middle XYZ of Current View
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public static XYZ GetMiddleOfActiveViewWindow(UIView view)
        {
            if (view == null) return new XYZ();

            var topLeft = view.GetZoomCorners()[0];
            var bottomRight = view.GetZoomCorners()[1];
            var width = bottomRight.X - topLeft.X;
            var height = bottomRight.Y - topLeft.Y;
            var middleX = bottomRight.X - (width / 2);
            var middleY = bottomRight.Y - (height / 2);
            var eyeHeight = height > width ? (height * 1.5) : width;
            return new XYZ(middleX, middleY, eyeHeight);
        }

        public static UIView ActiveUIView(UIDocument uiDoc, Element currentView)
        {
            if (uiDoc == null || currentView == null) return null;
            return (from openUIView in uiDoc.GetOpenUIViews()
                    let view = uiDoc.Document.GetElement(openUIView.ViewId)
                    where view.Name == currentView.Name
                    select openUIView).FirstOrDefault();
        }


    }
}
