// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

/* CreateCameraFromView.cs
 * prspkt.ru
 * © PRSPKT Architects, 2018
 *
 * This updater is used to create an updater capable of reacting
 * to changes in the Revit model.
 */

#region Namespaces

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using PRSPKT_Apps;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ArgumentException = Autodesk.Revit.Exceptions.ArgumentException;

#endregion


namespace UserView
{
    /// <summary>
    /// Revit external command.
    /// </summary>	
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.NoCommandData)]
    public class CreateCameraFromView : IExternalCommand
    {
        /// <summary>
        /// This method implements the external command within 
        /// Revit.
        /// </summary>
        /// <param name="commandData">An ExternalCommandData 
        /// object which contains reference to Application and 
        /// View needed by external command.</param>
        /// <param name="message">Error message can be returned
        /// by external command. This will be displayed only if
        /// the command status was "Failed". There is a limit 
        /// of 1023 characters for this message; strings longer
        /// than this will be truncated.</param>
        /// <param name="elements">Element set indicating 
        /// problem elements to display in the failure dialog. 
        /// This will be used only if the command status was 
        /// "Failed".</param>
        /// <returns>The result indicates if the execution 
        /// fails, succeeds, or was canceled by user. If it 
        /// does not succeed, Revit will undo any changes made 
        /// by the external command.</returns>	
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            var result = Result.Failed;

            if (commandData == null)
            {
                result = Result.Failed;
            }

            var uiApp = commandData?.Application;
            var uiDoc = uiApp?.ActiveUIDocument;
            var doc = uiDoc?.Document;

            using (var t = new Transaction(doc))
            {
                try
                {
                    CameraFromViewMethod(uiDoc, t);
                    result = Result.Succeeded;
                }
                catch (OperationCanceledException exceptionCancelled)
                {
                    message = exceptionCancelled.Message;
                    if (t.HasStarted())
                    {
                        t.RollBack();
                    }

                    result = Result.Cancelled;
                }
                catch (ErrorMessageException errorException)
                {
                    message = errorException.Message;
                    if (t.HasStarted())
                    {
                        t.RollBack();
                    }

                    result = Result.Failed;
                }
                catch (Exception ex)
                {
                    message = "Неожиданная ошибка: " + ex.Message;
                    if (t.HasStarted())
                    {
                        t.RollBack();
                    }

                    result = Result.Failed;
                }
            }

            return result;
        }

        private static void CameraFromViewMethod(UIDocument uiDoc, Transaction t)
        {
            var doc = uiDoc.Document;
            var currentView = doc.ActiveView;

            t.Start("Создание камеры из вида");
            switch (currentView.ViewType)
            {
                case ViewType.ThreeD:
                    CreatePerspectiveFrom3D(uiDoc, currentView as View3D);
                    break;
                case ViewType.FloorPlan:
                    CreatePerspectiveFromPlan(uiDoc, currentView);
                    break;
                case ViewType.Section:
                    CreatePerspectiveFromSection(uiDoc, currentView);
                    break;
                default:
                    using (var td = new TaskDialog("Ошибка"))
                    {
                        td.MainInstruction = "Ошибка";
                        td.MainContent =
                            "В настоящее время камеры могут быть созданы только из 3Д или планов видов \n";
                        td.Show();
                    }
                    break;
            }

            t.Commit();
        }


        private static void CreatePerspectiveFromSection(UIDocument uiDoc, View currentView)
        {
            UIView view = UserView.ActiveUIView(uiDoc, currentView);
            var eye = UserView.GetMiddleOfActiveViewWindow(view);
            var up = new XYZ(0, 0, -1);
            var forward = new XYZ(0, 0, -1);
            var viewOrientation3D = new ViewOrientation3D(eye, up, forward);
            var view3D = View3D.CreatePerspective(uiDoc.Document, UserView.Get3DViewFamilyTypes(uiDoc.Document).First().Id);
            view3D.SetOrientation(new ViewOrientation3D(viewOrientation3D.EyePosition,
                viewOrientation3D.UpDirection,
                viewOrientation3D.ForwardDirection));
            UserView.ApplySectionBoxToView(UserView.SectionViewExtentsBoundingBox(view), view3D);
            view3D.Name = UserView.GetNewViewName(uiDoc.Document, view3D);
            view3D.ViewTemplateId = ElementId.InvalidElementId;
            viewOrientation3D.Dispose();
        }

        private static void CreatePerspectiveFromPlan(UIDocument uiDoc, View currentView)
        {
            var view = UserView.ActiveUIView(uiDoc, currentView);
            var eye = UserView.GetMiddleOfActiveViewWindow(view);
            var up = new XYZ(0, 1, 0);
            var forward = new XYZ(0, 0, -1);
            var viewOrientation3D = new ViewOrientation3D(eye, up, forward);
            var view3D = View3D.CreatePerspective(uiDoc.Document, UserView.Get3DViewFamilyTypes(uiDoc.Document).First().Id);
            view3D.SetOrientation(new ViewOrientation3D(viewOrientation3D.EyePosition,
                viewOrientation3D.UpDirection,
                viewOrientation3D.ForwardDirection));
            UserView.ApplySectionBoxToView(UserView.ViewExtentsBoundingBox(view), view3D);
            view3D.Name = UserView.GetNewViewName(uiDoc.Document, view3D);
            view3D.ViewTemplateId = ElementId.InvalidElementId;
            viewOrientation3D.Dispose();
        }

        private static void CreatePerspectiveFrom3D(UIDocument uiDoc, View3D view3D)
        {
            var viewOrientation3D = view3D.GetOrientation();
            var centerOfScreen = UserView.GetMiddleOfActiveViewWindow(UserView.ActiveUIView(uiDoc, view3D));
            var view = View3D.CreatePerspective(uiDoc.Document,
                UserView.Get3DViewFamilyTypes(uiDoc.Document).First().Id);
            view.SetOrientation(new ViewOrientation3D(
                new XYZ(centerOfScreen.X, centerOfScreen.Y, viewOrientation3D.EyePosition.Z),
                viewOrientation3D.UpDirection, viewOrientation3D.ForwardDirection));
            view.Dispose();
            viewOrientation3D.Dispose();
        }
    }
}