// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

#region Namespaces
using Autodesk.Revit.DB;
using PRSPKT_Apps;
using PRSPKT_Apps.Dimensions;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace Dimensions
{
    public static class Creater
    {

        public static void CreateDimensionElement(Document doc, List<Dimenser> dimList, List<Grid> gridList, Options opt)
        {
            ReferenceArray referenceArray = PopulateReferenceArray(doc, dimList, gridList, opt);
            Dimension dim = (doc.Create.NewDimension(doc.ActiveView, dimList[0].MidLine, referenceArray));
            Tools.BeatyDimension(ref dim, dimList[0].MidLine.Direction.X, dimList[0].MidLine.Direction.Y, doc);
        }

        /// <summary>
        /// Converts a ReferenceArray to array with a bunch of lines
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="dimList"></param>
        /// <param name="gridList"></param>
        /// <param name="opt"></param>
        /// <returns></returns>
        private static ReferenceArray PopulateReferenceArray(Document doc, List<Dimenser> dimList, List<Grid> gridList, Options opt)
        {
            var tempArray = new ReferenceArray();
            if (doc.ActiveView.ViewType == ViewType.Section)
            {
                dimList.OrderBy(wall => wall.Height).ToList();
            }

            foreach (Dimenser dimenser in dimList)
            {
                tempArray.Append(dimenser.Face[0].Reference);
                tempArray.Append(dimenser.Face[1].Reference);
            }
            opt.IncludeNonVisibleObjects = true;
            opt.View = doc.ActiveView;

            foreach (Grid grid in gridList)
            {
                foreach (GeometryObject obj in grid.get_Geometry(opt))
                {
                    if (obj is Line line)
                    {
                        tempArray.Append(((Curve)line).Reference);
                    }
                }
            }
            return tempArray;
        }

        public static void CreateDimensionElementOnSec(Document doc, List<Dimenser> dimList, List<Grid> gridList, Options opt)
        {
            ReferenceArray referenceArray = PopulateReferenceArray(doc, dimList, gridList, opt);
            Dimension dim = (doc.Create.NewDimension(doc.ActiveView, dimList[0].MidLine, referenceArray));
            Tools.BeatyDimensionSection(ref dim, dimList[0].MidLine.Direction.X, dimList[0].MidLine.Direction.Y, doc);
        }
    }
}
