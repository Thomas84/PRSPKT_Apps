// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using PRSPKT_Apps;
using PRSPKT_Apps.Common;
using PRSPKT_Apps.FloorEdit;
using System;
using System.Linq;

namespace FloorEdit
{
    [Transaction(TransactionMode.Manual)]
    public class Main : IExternalCommand
    {
        public double Offset { get; set; }

        public double FloorPos { get; set; }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiApp = commandData.Application;
            var uiDoc = commandData.Application.ActiveUIDocument;
            var doc = uiDoc.Document;


            using (var t = new Transaction(doc))
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

        private static double GetEdgeLength(XYZ p2, XYZ p1)
        {
            return Math.Sqrt(Math.Pow((p2.X - p1.X), 2) + Math.Pow((p2.Y - p1.Y), 2));
        }

        private void FloorEdit(UIDocument uIdoc, Transaction t)
        {
            var doc = uIdoc.Document;
            t.Start("Уклоны");
            var userControl = new FloorEditControl(uIdoc);
            userControl.InitializeComponent();
            userControl.txtBox_userNumber.Text = UserSettings.Get("floorEdit_userNumber");
            var type = UserSettings.Get("floorEdit_type");
            if (type == "0")
            {
                userControl.rOnePoint.IsChecked = true;
            }
            else
            {
                userControl.rEdge.IsChecked = true;
            }

            userControl.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            if (userControl.ShowDialog() == true)
            {
                Floor floor = doc.GetElement(uIdoc.Selection.PickObject(ObjectType.Element, new FloorSelectionFilter(), "Выберите пол")) as Floor;
                floor.SlabShapeEditor.Enable();
                FloorPos = ((Level)doc.GetElement(floor.LevelId)).Elevation + floor.get_Parameter(BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM).AsDouble();
                var vertices = floor.SlabShapeEditor.SlabShapeVertices;


                if (userControl.rEdge.IsChecked == true)
                {
                    Reference r = uIdoc.Selection.PickObject(ObjectType.Edge, "Выберите грань");
                    Element e = doc.GetElement(r.ElementId);
                    Edge edge = e.GetGeometryObjectFromReference(r) as Edge;
                    Curve curve = edge.AsCurve();
                    XYZ p1 = curve.GetEndPoint(0);
                    XYZ p2 = curve.GetEndPoint(1);
                    XYZ p3 = uIdoc.Selection.PickObject(ObjectType.PointOnElement, "Выберите точку").GlobalPoint;

                    var a = GetEdgeLength(p1, p2); // длина стороны треугольника
                    var b = GetEdgeLength(p1, p3); // длина стороны треугольника
                    var c = GetEdgeLength(p2, p3); // длина стороны треугольника

                    var p = (a + b + c) / 2; // полупериметр
                    var H = 2 / a * Math.Sqrt(p * (p - a) * (p - b) * (p - c)); // высота треугольника
                    var x = b > H ? Math.Sqrt(b * b - H * H) : Math.Sqrt(H * H - b * b);
                    var xZ = Tools.IsZero(x) ? 0 : Math.Abs(x * (p1.Z - p2.Z) / a);

                    foreach (SlabShapeVertex vertex in vertices)
                    {
                        if (Tools.IsEqual(vertex.Position.X, p3.X) && Tools.IsEqual(vertex.Position.Y, p3.Y) &&
                            Tools.IsEqual(vertex.Position.Z, p3.Z))
                        {
                            Offset = H * (userControl.UserNumber / 100) + p1.Z - xZ - FloorPos; // определим смещение точки относительно координаты Z точки 1
                            floor.SlabShapeEditor.ModifySubElement(vertex, Offset);
                            break;
                        }
                    }
                }
                else
                {
                    XYZ p1 = uIdoc.Selection.PickObject(ObjectType.PointOnElement, "Выберите точку").GlobalPoint;
                    XYZ p2 = uIdoc.Selection.PickObject(ObjectType.PointOnElement, "Выберите точку").GlobalPoint;
                    var points = uIdoc.Selection.PickObjects(ObjectType.PointOnElement, "Точечки").Select(q => q.GlobalPoint);
                    var length = GetEdgeLength(p2, p1);
                    //offset = length * (userControl.UserNumber / 100) + (p1.Z - FloorPos);

                    foreach (SlabShapeVertex v in vertices)
                    {

                        if (!(Tools.IsEqual(v.Position.X, p1.X) && Tools.IsEqual(v.Position.Y, p1.Y) && Tools.IsEqual(v.Position.Z, p1.Z)))
                        {
                            if ((Tools.IsEqual(v.Position.X, p2.X) && Tools.IsEqual(v.Position.Y, p2.Y) && Tools.IsEqual(v.Position.Z, p2.Z)))
                            {
                                Offset = length * (userControl.UserNumber / 100) + (p1.Z - FloorPos);
                                floor.SlabShapeEditor.ModifySubElement(v, Offset);
                                break;
                            }
                        }
                    }

                    if (points.Any())
                    {
                        foreach (var point in points)
                        {
                            foreach (SlabShapeVertex v in vertices)
                            {
                                if (!Tools.IsEqual(v.Position.X, point.X) || !Tools.IsEqual(v.Position.Y, point.Y) ||
                                    !Tools.IsEqual(v.Position.Z, point.Z)) continue;
                                length = GetEdgeLength(p1, v.Position);
                                Offset = length * (userControl.UserNumber / 100) + (p1.Z - FloorPos);
                                floor.SlabShapeEditor.ModifySubElement(v, Offset);

                            }
                        }
                    }
                }

            }
            t.Commit();

            UserSettings.Set("floorEdit_userNumber", userControl.txtBox_userNumber.Text);
            UserSettings.Set("floorEdit_type",
                userControl.rOnePoint.IsChecked != null && (userControl.rOnePoint.IsChecked.Value)
                    ? "0"
                    : "1");
        }
    }


}
