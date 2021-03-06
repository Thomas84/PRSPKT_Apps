﻿// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

namespace DimAxies
{
    [Transaction(TransactionMode.Manual)]
    public class DimAxies : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            IList<ElementId> sel = new List<ElementId>(uidoc.Selection.GetElementIds());

            List<XYZ> dimLocPoint = new List<XYZ>();

            try
            {
                using (Transaction t = new Transaction(doc, "create dimension"))
                {
                    t.Start();
                    ReferenceArray referenceArray = new ReferenceArray();
                    Options opt = new Options();
                    opt.ComputeReferences = true;
                    opt.IncludeNonVisibleObjects = true;
                    opt.View = doc.ActiveView;
                    foreach (ElementId e in sel)
                    {
                        Grid grid = doc.GetElement(e) as Grid;
                        foreach (GeometryObject obj in grid.get_Geometry(opt))
                        {
                            if (obj is Line)
                            {
                                Line gridline = obj as Line;
                                referenceArray.Append(gridline.Reference);

                                if (dimLocPoint.Count < 2)
                                {
                                    if (dimLocPoint.Count == 0)
                                    {
                                        dimLocPoint.Add(gridline.GetEndPoint(0));
                                    }
                                    else
                                    {
                                        gridline.MakeUnbound();
                                        dimLocPoint.Add(gridline.Project(dimLocPoint[0]).XYZPoint);
                                    }
                                }
                            }
                        }
                    }

                    doc.Create.NewDimension(doc.ActiveView, Line.CreateBound(dimLocPoint[0], dimLocPoint[1]), referenceArray);

                    t.Commit();
                }
                return Result.Succeeded;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch (Exception e)
            {
                TaskDialog.Show("Error", e.Message);
                return Result.Failed;

            }
        }

    }

}
