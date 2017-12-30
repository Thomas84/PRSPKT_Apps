// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;

namespace PRSPKT_Apps.Dimensions
{
    public class Dimenser
    {
        #region initialize variables
        private List<PlanarFace> face = new List<PlanarFace>();
        private Wall wall;
        private Line geoLine;
        private double thickness;
        private double height;
        private XYZ midPoint1;
        private XYZ midPoint;
        private double directionX;
        private double directionY;
        private XYZ xyz;
        private Line midLine;

        public double Thickness { get => thickness; set => thickness = value; }
        public double Height { get => height; set => height = value; }
        public Wall Wall { get => wall; set => wall = value; }
        public double DirectionX { get => directionX; set => directionX = value; }
        public double DirectionY { get => directionY; set => directionY = value; }
        public List<PlanarFace> Face { get => face; set => face = value; }
        public XYZ Xyz { get => xyz; set => xyz = value; }
        public Line MidLine { get => midLine; set => midLine = value; }

        #endregion

        public Dimenser(Document doc, Element wall, Options opt)
        {

            foreach (Parameter item in doc.GetElement(wall.GetTypeId()).Parameters)
            {
                if (item.Definition.Name == "Ширина")
                {
                    thickness = item.AsDouble();
                }
            }

            foreach (Parameter item in doc.GetElement(wall.Id).Parameters)
            {
                if (item.Definition.Name == "Неприсоединенная высота")
                {
                    height = item.AsDouble();
                }
            }

            ReferenceArray reference = new ReferenceArray();
            this.Wall = (Wall)wall;
            this.geoLine = (wall.Location as LocationCurve).Curve as Line;
            this.DirectionX = Math.Round(this.geoLine.Direction.Y, 4);
            this.DirectionY = Math.Round(this.geoLine.Direction.X, 4);
            this.xyz = Tools.Midpoint(this.geoLine);
            this.midPoint = new XYZ(xyz.X + Thickness / 2.0 * DirectionX, xyz.Y + Thickness / 2.0 * DirectionY, xyz.Z + Height / 2.0);
            this.midPoint1 = new XYZ(xyz.X - Thickness / 2.0 * DirectionX, xyz.Y - Thickness / 2.0 * DirectionY, xyz.Z + Height / 2.0);
            var MidLine = Line.CreateBound(this.midPoint, this.midPoint1);

            foreach (PlanarFace planarFace in Tools.GetFacesAndEdges(wall, opt))
            {
                if (Tools.IsEqual(Tools.RoundAbsValue(planarFace.XVector.X), Tools.RoundAbsValue(geoLine.Direction.X)) &&
                    Tools.IsEqual(Tools.RoundAbsValue(planarFace.XVector.Y), Tools.RoundAbsValue(geoLine.Direction.Y)) &&
                    Tools.IsZero(Tools.RoundAbsValue(planarFace.FaceNormal.Z)))
                {
                    Face.Add(planarFace);
                    reference.Append(planarFace.Reference);
                }
            }

        }
    }
}
