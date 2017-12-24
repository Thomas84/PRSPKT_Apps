using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using PRSPKT_Apps;
using PRSPKT_Apps.FloorEdit;
using System;
using System.Collections.Generic;

namespace FloorEdit
{
    [Transaction(TransactionMode.Manual)]
    public class Main : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;


            using (Transaction t = new Transaction(doc))
            {
                try
                {
                    FloorEdit(uiDoc, t);
                    return Result.Succeeded;
                }
                catch (OperationCanceledException exceptionCancelled)
                {
                    message = exceptionCancelled.Message;
                    if (t.HasStarted())
                    {
                        t.RollBack();
                    }
                    return Result.Cancelled;
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

        private double GetEdgeLength(XYZ p2, XYZ p1)
        {
            return Math.Sqrt(Math.Pow((p2.X - p1.X), 2) + Math.Pow((p2.Y - p1.Y), 2));
        }

        private void FloorEdit(UIDocument uIdoc, Transaction t)
        {
            Document _doc = uIdoc.Document;
            t.Start("Уклоны");
            var userControl = new PRSPKT_Apps.FloorEdit.FloorEditControl(uIdoc);
            userControl.InitializeComponent();
            userControl.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            if (userControl.ShowDialog() == true)
            {
                Floor floor = _doc.GetElement(uIdoc.Selection.PickObject(ObjectType.Element, new FloorSelectionFilter(), "Выберите пол")) as Floor;
                floor.SlabShapeEditor.Enable();
                Reference r = uIdoc.Selection.PickObject(ObjectType.Edge, "Выберите грань");
                Element e = _doc.GetElement(r.ElementId);
                Edge edge = e.GetGeometryObjectFromReference(r) as Edge;
                Curve curve = edge.AsCurve();
                XYZ p1 = curve.GetEndPoint(0);
                XYZ p2 = curve.GetEndPoint(1);
                XYZ p3 = uIdoc.Selection.PickObject(ObjectType.PointOnElement, "Выберите точку").GlobalPoint;

                double a = GetEdgeLength(p1, p2);
                double b = GetEdgeLength(p1, p3);
                double c = GetEdgeLength(p2, p3);

                double p = (a + b + c) / 2;
                double H = 2 / a * Math.Sqrt(p * (p - a) * (p - b) * (p - c));
                double x = Math.Sqrt(b * b - H * H);


                double offset = H * (userControl.UserNumber / 100) + p1.Z;
                var vertices = floor.SlabShapeEditor.SlabShapeVertices;
                foreach (SlabShapeVertex vertex in vertices)
                {
                    if (vertex.Position.X == p3.X && vertex.Position.Y == p3.Y && vertex.Position.Z == p3.Z)
                    {
                        floor.SlabShapeEditor.ModifySubElement(vertex, offset);
                        break;
                    }
                }
            }
            t.Commit();

        }
    }


}
