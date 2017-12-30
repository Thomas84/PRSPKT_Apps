// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using PRSPKT_Apps.Dimensions;
using System;
using System.Collections.Generic;

namespace Dimensions
{
    [Transaction(TransactionMode.Manual)]
    public class WallDimension : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument UIdoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = UIdoc.Document;
            Options opt = app.Create.NewGeometryOptions();
            opt.ComputeReferences = true;

            List<Wall> WallList = new List<Wall>();
            List<Grid> GridList1 = new List<Grid>();
            List<Grid> GridList2 = new List<Grid>();
            List<Dimenser> List1 = new List<Dimenser>();
            List<Dimenser> List2 = new List<Dimenser>();
            List<Grid> GridList = new List<Grid>();
            List<Dimenser> dimInXdirection = new List<Dimenser>();
            List<Dimenser> dimInYdirection = new List<Dimenser>();
            List<Grid> gridInXdirection = new List<Grid>();
            List<Grid> gridInYdirection = new List<Grid>();

            IList<Element> ElementList = UIdoc.Selection.PickElementsByRectangle(new Utils.GridWallSelectionFilter(), "Выберите стены");

            if (ElementList.Count == 0)
                return Result.Cancelled;

            for (int index = 0; index < ElementList.Count; ++index)
            {
                if (ElementList[index] is Wall)
                    WallList.Add(ElementList[index] as Wall);
                else
                    GridList.Add((Grid)ElementList[index]);
            }

            foreach (Wall wall in WallList)
            {
                Dimenser dimenser = new Dimenser(doc, wall, opt);
                if (Math.Round(dimenser.DirectionX, 4) != 0.0)
                {
                    dimInXdirection.Add(dimenser);
                }
                else dimInYdirection.Add(dimenser);
            }

            foreach (Grid grid in GridList)
            {
                if (Math.Round(((Line)grid.Curve).Direction.Y, 4) != 0.0)
                {
                    gridInYdirection.Add(grid);
                }
                else gridInXdirection.Add(grid);
            }

            using (var t = new Transaction(doc, "Простановка размеров"))
            {
                t.Start();
                if (doc.ActiveView.ViewType == ViewType.Section)
                {
                    if (dimInXdirection.Count > 0)
                    {
                        Creater.CreateDimensionElementOnSec(doc, dimInXdirection, gridInYdirection, opt); // потому что стены и оси перпендикулярные
                    }
                    if (dimInYdirection.Count > 0)
                    {
                        Creater.CreateDimensionElementOnSec(doc, dimInYdirection, gridInXdirection, opt); // потому что стены и оси перпендикулярные
                    }
                }
                else
                {
                    if (dimInXdirection.Count > 0)
                    {
                        Creater.CreateDimensionElement(doc, dimInXdirection, gridInYdirection, opt);
                    }
                    if (dimInYdirection.Count > 0)
                    {
                        Creater.CreateDimensionElement(doc, dimInYdirection, gridInXdirection, opt);
                    }
                }
                t.Commit();
            }
            return Result.Succeeded;
        }
    }
}
