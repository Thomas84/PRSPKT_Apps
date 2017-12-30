// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using PRSPKT_Apps;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dimensions
{
    [Transaction(TransactionMode.Manual)]
    public class DimensionTolerance : IExternalCommand
    {
        private double directionX;
        private double directionY;

        public double DirectionX { get => directionX; set => directionX = value; }
        public double DirectionY { get => directionY; set => directionY = value; }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument UIdoc = commandData.Application.ActiveUIDocument;
            Document doc = UIdoc.Document;
            Application app = uiApp.Application;

            Options options = app.Create.NewGeometryOptions();
            options.ComputeReferences = true;

            IList<Dimension> dimensionList = UIdoc.Selection.PickElementsByRectangle(new Utils.DimSelectionFilter(), "Выберите размерные линии")
                .Select(dim => dim as Dimension).ToList();

            if (dimensionList.Count == 0)
                return Result.Cancelled;

            using (var t = new Transaction(doc, "Dim"))
            {
                t.Start();
                if (UIdoc.ActiveView.ViewType == ViewType.Section)
                {
                    for (int i = 0; i < dimensionList.Count; i++)
                    {
                        Dimension dim = dimensionList[i];
                        Line curve = (Line)dim.Curve;
                        directionX = Math.Round(curve.Direction.X, 4);
                        directionY = Math.Round(curve.Direction.Y, 4);
                        Tools.BeatyDimensionSection(ref dim, DirectionX, DirectionY, doc);
                        dimensionList[i] = dim;
                    }
                }
                else
                {
                    for (int i = 0; i < dimensionList.Count; ++i)
                    {
                        Dimension dim = dimensionList[i];
                        Line curve = (Line)dim.Curve;
                        directionX = Math.Round(curve.Direction.X, 4);
                        directionY = Math.Round(curve.Direction.Y, 4);
                        Tools.BeatyDimension(ref dim, DirectionX, DirectionY, doc);
                        dimensionList[i] = dim;
                    }
                }
                t.Commit();

            }
            return Result.Succeeded;
        }
    }
}
