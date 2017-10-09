using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PRSPKT_Apps.UserView
{
    class CameraFromViewCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            if (commandData == null)
            {
                return Result.Failed;
            }

            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            View currentView = doc.ActiveView;

            switch (currentView.ViewType)
            {
                case ViewType.FloorPlan:
                    CreatePerspectiveFromPlan(uidoc, currentView);
                    break;
                case ViewType.CeilingPlan:
                    break;
                case ViewType.Section:
                    CreatePerspectiveFromSection(uidoc, currentView);
                    break;
                case ViewType.ThreeD:
                    CreatePerspectiveFrom3D(uidoc, currentView as View3D);
                    break;

                default:
                    using (TaskDialog td = new TaskDialog("Camera Tools"))
                    {
                        td.MainInstruction = "AHTUNG!";
                        td.MainContent = "Инструмент работает только в 3д или видах в плане";
                        td.Show();

                    }
                    break;
            }
            return Result.Succeeded;
        }

        private void CreatePerspectiveFrom3D(UIDocument uidoc, View3D view3D)
        {
            ViewOrientation3D viewOrientation3D = view3D.GetOrientation();
            View3D newView;
            using (var t = new Transaction(uidoc.Document))
            {
                t.Start("Создание перспективного вида");
                XYZ centerOfScreen = GetMiddleOfActiveView(ActiveUIView(uidoc, (View)view3D));
                newView = View3D.CreatePerspective(uidoc.Document,
                    Get3DViewFamilyTypes(uidoc.Document).First().Id);
                newView.SetOrientation(new ViewOrientation3D(new XYZ(centerOfScreen.X,
                    centerOfScreen.Y, viewOrientation3D.EyePosition.Z),
                    viewOrientation3D.UpDirection,
                    viewOrientation3D.ForwardDirection));
                t.Commit();
                newView.Dispose();
            }
            view3D.Dispose();
        }

        private static List<ViewFamilyType> Get3DViewFamilyTypes(Document document)
        {
            var viewFamilyTypes = new List<ViewFamilyType>();
            using (var collector = new FilteredElementCollector(document))
            {
                collector.OfClass(typeof(ViewFamilyType));
                foreach (ViewFamilyType viewFamilyType in collector)
                {
                    if (viewFamilyType.ViewFamily == ViewFamily.ThreeDimensional)
                    {
                        viewFamilyTypes.Add(viewFamilyType);
                    }
                }
            }
            return viewFamilyTypes;
        }

        /// <summary>
        /// Create Perspective view from Section view
        /// </summary>
        /// <param name="uidoc"></param>
        /// <param name="sectionView"></param>
        private void CreatePerspectiveFromSection(UIDocument uidoc, View sectionView)
        {
            UIView view = ActiveUIView(uidoc, sectionView);
            XYZ eye = GetMiddleOfActiveView(view);
            XYZ up = new XYZ(0, 0, 1);
            XYZ forward = new XYZ(0, 0, -1);
            ViewOrientation3D viewOrientation3D = new ViewOrientation3D(eye, up, forward);
            using (var t = new Transaction(uidoc.Document))
            {
                t.Start("Создание перспективного вида");
                View3D newView = View3D.CreatePerspective(
                    uidoc.Document,
                    Get3DViewFamilyTypes(uidoc.Document).First().Id);
                newView.SetOrientation(new ViewOrientation3D(
                    viewOrientation3D.EyePosition,
                    viewOrientation3D.UpDirection,
                    viewOrientation3D.ForwardDirection));
                ApplySectionBoxToView(SectionViewExtentsBoudingBox(view), newView);
                t.Commit();
            }
        }

        private void CreatePerspectiveFromPlan(UIDocument uidoc, View currentView)
        {
            UIView uiview = ActiveUIView(uidoc, currentView);
            XYZ eye = GetMiddleOfActiveView(uiview);
            XYZ up = new XYZ(0, 0, 1);
            XYZ forward = new XYZ(0, 0, -1);
            ViewOrientation3D viewOrientation3D = new ViewOrientation3D(eye, up, forward);
            using (var t = new Transaction(uidoc.Document))
            {
                t.Start("Создание перспективного вида");
                View3D newView = View3D.CreatePerspective(uidoc.Document, Get3DViewFamilyTypes(uidoc.Document).First().Id);
                newView.SetOrientation(new ViewOrientation3D(viewOrientation3D.EyePosition,
                    viewOrientation3D.UpDirection,
                    viewOrientation3D.ForwardDirection));
                ApplySectionBoxToView(ViewViewExtentsBoudingBox(uiview), newView);
                t.Commit();
            }

        }

        public static BoundingBoxXYZ ViewViewExtentsBoudingBox(UIView uiview)
        {
            if (uiview == null)
            {
                return new BoundingBoxXYZ();

            }
            BoundingBoxXYZ result = new BoundingBoxXYZ();
            XYZ min = new XYZ(uiview.GetZoomCorners()[0].X, uiview.GetZoomCorners()[0].Y, uiview.GetZoomCorners()[0].Z - 4);
            XYZ max = new XYZ(uiview.GetZoomCorners()[1].X, uiview.GetZoomCorners()[1].Y, uiview.GetZoomCorners()[0].Z + 4);
            result.set_Bounds(0, min);
            result.set_Bounds(1, max);
            return result;
        }

        public static BoundingBoxXYZ SectionViewExtentsBoudingBox(UIView uiview)
        {
            if (uiview == null)
            {
                return new BoundingBoxXYZ();
            }
            BoundingBoxXYZ result = new BoundingBoxXYZ();
            try
            {
                XYZ min = new XYZ(uiview.GetZoomCorners()[0].X, uiview.GetZoomCorners()[0].Y, uiview.GetZoomCorners()[0].Z - 4);
                XYZ max = new XYZ(uiview.GetZoomCorners()[1].X, uiview.GetZoomCorners()[1].Y, uiview.GetZoomCorners()[0].Z + 4);
                result.set_Bounds(0, min);
                result.set_Bounds(1, max);
            }
            catch (ArgumentException e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                result.Dispose();
                return null;
            }
            return result;
        }

        public static void ApplySectionBoxToView(BoundingBoxXYZ bounds, View3D view)
        {
            if (bounds != null || view != null)
            {
                view.SetSectionBox(bounds);
            }
        }

        public static XYZ GetMiddleOfActiveView(UIView uiview)
        {
            if (uiview == null)
            {
                return new XYZ();
            }
            XYZ topLeft = uiview.GetZoomCorners()[0];
            XYZ bottomRight = uiview.GetZoomCorners()[1];
            double width = bottomRight.X - topLeft.X;
            double height = bottomRight.Y - topLeft.Y;
            double middleX = bottomRight.X - (width / 2);
            double middleY = bottomRight.Y - (height / 2);
            double eyeHeight = height > width ? (height * 1.5) : width;
            return new XYZ(middleX, middleY, eyeHeight);
        }

        public static UIView ActiveUIView(UIDocument uidoc, View currentView)
        {
            if (uidoc != null && currentView != null)
            {
                foreach (UIView uview in uidoc.GetOpenUIViews())
                {
                    View view = (View)uidoc.Document.GetElement(uview.ViewId);
                    if (view.Name == currentView.Name)
                    {
                        return uview;
                    }
                }
            }
            return null;
        }
    }
}
