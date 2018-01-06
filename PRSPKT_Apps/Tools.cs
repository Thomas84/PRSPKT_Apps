// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;

namespace PRSPKT_Apps
{
    public static class Tools
    {
        public static double Eps { get; } = 1.0e-9;

        // Define CultureInfo
        public static ResourceManager LangResMan;
        public static CultureInfo Cult;



        public static string GetResourceManager(string _path)
        {
            var path = _path;
            return Tools.LangResMan.GetString(path, Tools.Cult);
        }

        public static void GetLocalisationValues()
        {
            Tools.Cult = CultureInfo.CreateSpecificCulture("ru");
            Tools.LangResMan = new ResourceManager("PRSPKT_Apps.Resources.ru", System.Reflection.Assembly.GetExecutingAssembly());
        }

        public static double? GetValueFromString(string text, Units units)
        {
            // Check the string value
            var heightValueString = text;

            if (UnitFormatUtils.TryParse(units, UnitType.UT_Length, heightValueString, out double length))
            {
                return length;
            }
            else return null;
        }

        public static bool IsZero(
          double a,
          double tolerance)
        {
            return tolerance > Math.Abs(a);
        }

        public static bool IsZero(double a)
        {
            return IsZero(a, Eps);
        }

        public static bool IsEqual(double a, double b)
        {
            return IsZero(b - a);
        }

        /// <summary>
        /// Return the midpoint between two points.
        /// </summary>
        /// <param name="p">Point 1</param>
        /// <param name="q">Point 2</param>
        /// <returns></returns>
        public static XYZ Midpoint(XYZ p, XYZ q)
        {
            return (q + p) / 2;
        }
        /// <summary>
        /// Return the midpoint of a Line.
        /// </summary>
        /// <param name="line">Line</param>
        /// <returns></returns>
        public static XYZ Midpoint(Line line)
        {
            return Tools.Midpoint(((Curve)line).GetEndPoint(0), ((Curve)line).GetEndPoint(1));
        }
        /// <summary>
        /// Get Faces and Edges
        /// </summary>
        /// <param name="wall"></param>
        /// <param name="opt"></param>
        /// <returns></returns>
        public static List<PlanarFace> GetFacesAndEdges(Element wall, Options opt)
        {
            var planarFaceList = new List<PlanarFace>();

            foreach (var geo in wall.get_Geometry(opt))
            {
                var solid = geo as Solid;
                if (solid == null) continue;
                foreach (Face face in solid.Faces)
                {
                    var planarFace = face as PlanarFace;
                    planarFaceList.Add(planarFace);
                }
            }
            return planarFaceList;
        }

        private static readonly double MoveDimText = MmToFt(200.0);

        public static double RoundAbsValue(double value)
        {
            return Math.Abs(Math.Round(value, 4));
        }

        public static void BeatyDimension(ref Dimension dim, double DirectionX, double DirectionY, Document doc)
        {
            double scale = Math.Round(doc.ActiveView.Scale / 100.0, 2);
            double scaleMm = MmToFt(400.0) * scale;
            if (dim.NumberOfSegments > 1)
            {
                List<DimensionSegment> dimensionSegmentList = new List<DimensionSegment>();

                foreach (DimensionSegment dimSeg in dim.Segments)
                {
                    dimensionSegmentList.Add(dimSeg);
                }

                double? nullable = dimensionSegmentList[0].Value;
                //double num3 = scaleMm; // TODO
                if (nullable.HasValue && nullable.GetValueOrDefault() < scaleMm)
                {
                    DimensionSegment dimensionSegment = dimensionSegmentList[0];
                    //double x = dimensionSegmentList[0].TextPosition.X;
                    //nullable = dimensionSegmentList[0].Value;
                    double num4 = nullable.Value + MoveDimText * scale;
                    //nullable = dimensionSegmentList[0].Value;
                    double num5 = nullable.Value;
                    double num6 = (num4 + num5) * DirectionX;
                    double num7 = dimensionSegment.TextPosition.X - num6;
                    //double y = dimensionSegmentList[0].TextPosition.Y;
                    //nullable = dimensionSegmentList[0].Value;
                    double num8 = nullable.Value + MoveDimText * scale;
                    //nullable = dimensionSegmentList[0].Value;
                    double num9 = nullable.Value;
                    double num10 = (num8 + num9) * DirectionY;
                    double num11 = dimensionSegment.TextPosition.Y - num10;
                    //double num12 = 0.0;7
                    XYZ xyz = new XYZ(num7, num11, 0.0);
                    dimensionSegment.TextPosition = xyz;
                }
                nullable = dimensionSegmentList.Last().Value;
                double num13 = scaleMm;
                if (nullable.HasValue && nullable.GetValueOrDefault() < num13)
                {
                    DimensionSegment dimensionSegment = dimensionSegmentList.Last();
                    double x = dimensionSegmentList.Last().TextPosition.X;
                    nullable = dimensionSegmentList.Last().Value;
                    double num4 = nullable.Value + MoveDimText * scale;
                    nullable = dimensionSegmentList.Last().Value;
                    double num5 = nullable.Value;
                    double num6 = (num4 + num5) * DirectionX;
                    double num7 = x + num6;
                    double y = dimensionSegmentList.Last().TextPosition.Y;
                    nullable = dimensionSegmentList.Last().Value;
                    double num8 = nullable.Value + MoveDimText * scale;
                    nullable = dimensionSegmentList.Last().Value;
                    double num9 = nullable.Value;
                    double num10 = (num8 + num9) * DirectionY;
                    double num11 = y + num10;
                    double num12 = 0.0;
                    XYZ xyz = new XYZ(num7, num11, num12);
                    dimensionSegment.TextPosition = xyz;
                }
                dimensionSegmentList.Reverse();
                for (int index = 1; index < dimensionSegmentList.Count - 1; ++index)
                {
                    nullable = dimensionSegmentList[index].Value;
                    double num4 = nullable.Value;
                    XYZ textPosition = dimensionSegmentList[index].TextPosition;
                    int num5 = -1;
                    int num6 = 1;
                    while (true)
                    {
                        nullable = dimensionSegmentList[index].Value;
                        if (nullable.Value < scaleMm && index < dimensionSegmentList.Count - 1)
                        {
                            if (Tools.IsZero(DirectionY))
                            {
                                if (index < dimensionSegmentList.Count - 1)
                                {
                                    double num7 = 1.5 * scale;
                                    dimensionSegmentList[index].TextPosition = new XYZ(textPosition.X + num7 * (double)num6, textPosition.Y + num7 * (double)num6, textPosition.Z);
                                    nullable = dimensionSegmentList[index + 1].Value;
                                    if (nullable.Value < scaleMm)
                                    {
                                        ++index;
                                        if (index < dimensionSegmentList.Count - 1)
                                        {
                                            textPosition = dimensionSegmentList[index].TextPosition;
                                            dimensionSegmentList[index].TextPosition = new XYZ(textPosition.X + num7 * (double)num6, textPosition.Y - num7 * (double)num6, textPosition.Z);
                                        }
                                        else
                                            break;
                                    }
                                    num6 *= num5;
                                    if (index < dimensionSegmentList.Count - 1)
                                    {
                                        ++index;
                                        nullable = dimensionSegmentList[index].Value;
                                        if (nullable.Value >= scaleMm)
                                            break;
                                    }
                                    else
                                        break;
                                }
                                else
                                    break;
                            }
                            else if (index < dimensionSegmentList.Count - 1)
                            {
                                double num7 = 1.5 * scale;
                                dimensionSegmentList[index].TextPosition = new XYZ(textPosition.X - num7 * (double)num6, textPosition.Y - num7 * (double)num6, textPosition.Z);
                                nullable = dimensionSegmentList[index + 1].Value;
                                double num8 = scaleMm;
                                if (nullable.HasValue && nullable.GetValueOrDefault() < num8)
                                {
                                    ++index;
                                    if (index < dimensionSegmentList.Count - 1)
                                    {
                                        textPosition = dimensionSegmentList[index].TextPosition;
                                        dimensionSegmentList[index].TextPosition = new XYZ(textPosition.X + num7 * (double)num6, textPosition.Y - num7 * (double)num6, textPosition.Z);
                                    }
                                    else
                                        break;
                                }
                                num6 *= num5;
                                if (index < dimensionSegmentList.Count - 1)
                                {
                                    ++index;
                                    nullable = dimensionSegmentList[index].Value;
                                    double num9 = scaleMm;
                                    if (nullable.HasValue && nullable.GetValueOrDefault() >= num9)
                                        break;
                                }
                                else
                                    break;
                            }
                            else
                                break;
                        }
                        else
                            break;
                    }
                }

            }
            else
            {
                double? nullable = dim.Value;
                double num3 = scaleMm;
                if (nullable.HasValue && nullable.GetValueOrDefault() < num3)
                {
                    Dimension dimension = dim;
                    double x = dim.TextPosition.X;
                    nullable = dim.Value;
                    double num4 = nullable.Value;
                    double moveDimText = MoveDimText;
                    nullable = dim.Value;
                    double num5 = nullable.Value;
                    double num6 = moveDimText + num5;
                    double num7 = (num4 + num6) * DirectionX;
                    double num8 = x + num7;
                    double y = dim.TextPosition.Y;
                    nullable = dim.Value;
                    double num9 = nullable.Value + MoveDimText;
                    nullable = dim.Value;
                    double num10 = nullable.Value;
                    double num11 = (num9 + num10) * DirectionY;
                    double num12 = y + num11;
                    double num13 = 0.0;
                    XYZ xyz = new XYZ(num8, num12, num13);
                    dimension.TextPosition = xyz;
                }
            }
        }

        public static void BeatyDimensionSection(ref Dimension dim, double DirectionX, double DirectionY, Document doc)
        {
            double scale = Math.Round((double)doc.ActiveView.Scale / 100.0, 2);
            double scaleMm = MmToFt(400.0) * scale;
            if (dim.NumberOfSegments > 1)
            {
                var dimensionSegmentList = new List<DimensionSegment>();
                foreach (DimensionSegment dimSeg in dim.Segments)
                {
                    dimensionSegmentList.Add(dimSeg);
                }

                double? nullable;
                for (int index = 1; index < dimensionSegmentList.Count - 1; ++index)
                {
                    nullable = dimensionSegmentList[index].Value;
                    double num3 = nullable.Value;
                    XYZ textPosition = dimensionSegmentList[index].TextPosition;
                    int num4 = -1;
                    int num5 = 1;
                    while (true)
                    {
                        nullable = dimensionSegmentList[index].Value;
                        if (nullable.Value < scaleMm && index < dimensionSegmentList.Count - 1)
                        {
                            if (Tools.IsZero(DirectionY))
                            {
                                if (index < dimensionSegmentList.Count - 1)
                                {
                                    double num6 = 1.5 * scale;
                                    dimensionSegmentList[index].TextPosition = new XYZ(textPosition.X - num6 * (double)num5, textPosition.Y, textPosition.Z - num6 * (double)num5);
                                    nullable = dimensionSegmentList[index + 1].Value;
                                    if (nullable.Value < scaleMm)
                                    {
                                        ++index;
                                        if (index < dimensionSegmentList.Count - 1)
                                        {
                                            textPosition = dimensionSegmentList[index].TextPosition;
                                            dimensionSegmentList[index].TextPosition = new XYZ(textPosition.X - num6 * (double)num5, textPosition.Y, textPosition.Z + num6 * (double)num5);
                                        }
                                        else
                                            break;
                                    }
                                    num5 *= num4;
                                    if (index < dimensionSegmentList.Count - 1)
                                    {
                                        ++index;
                                        nullable = dimensionSegmentList[index].Value;
                                        if (nullable.Value >= scaleMm)
                                            break;
                                    }
                                    else
                                        break;
                                }
                                else
                                    break;
                            }
                            else if (index < dimensionSegmentList.Count - 1)
                            {
                                double num6 = 1.5 * scale;
                                dimensionSegmentList[index].TextPosition = new XYZ(textPosition.X, textPosition.Y - num6 * num5, textPosition.Z - num6 * num5);
                                nullable = dimensionSegmentList[index + 1].Value;
                                double num7 = scaleMm;
                                if (nullable.HasValue && nullable.GetValueOrDefault() < num7)
                                {
                                    ++index;
                                    if (index < dimensionSegmentList.Count - 1)
                                    {
                                        textPosition = dimensionSegmentList[index].TextPosition;
                                        dimensionSegmentList[index].TextPosition = new XYZ(textPosition.X, textPosition.Y + num6 * num5, textPosition.Z - num6 * num5);
                                    }
                                    else
                                        break;
                                }
                                num5 *= num4;
                                if (index < dimensionSegmentList.Count - 1)
                                {
                                    ++index;
                                    nullable = dimensionSegmentList[index].Value;
                                    double num8 = scaleMm;
                                    if (nullable.HasValue && nullable.GetValueOrDefault() >= num8)
                                        break;
                                }
                                else
                                    break;
                            }
                            else
                                break;
                        }
                        else
                            break;
                    }
                }
                if (Tools.IsZero(DirectionY))
                {
                    nullable = dimensionSegmentList[0].Value;
                    double num3 = scaleMm;
                    if (nullable.HasValue && nullable.GetValueOrDefault() < num3)
                    {
                        DimensionSegment dimensionSegment = dimensionSegmentList[0];
                        double x = dimensionSegmentList[0].TextPosition.X;
                        nullable = dimensionSegmentList[0].Value;
                        double num4 = nullable.Value + MoveDimText * scale;
                        nullable = dimensionSegmentList[0].Value;
                        double num5 = nullable.Value;
                        double num6 = (num4 + num5) * DirectionX;
                        XYZ xyz = new XYZ(x - num6, dimensionSegmentList[0].TextPosition.Y, dimensionSegmentList[0].TextPosition.Z);
                        dimensionSegment.TextPosition = xyz;
                    }
                    nullable = dimensionSegmentList.Last().Value;
                    double num7 = scaleMm;
                    if (!nullable.HasValue || nullable.GetValueOrDefault() >= num7)
                        return;
                    DimensionSegment dimensionSegment1 = dimensionSegmentList.Last();
                    double x1 = dimensionSegmentList.Last().TextPosition.X;
                    nullable = dimensionSegmentList.Last().Value;
                    double num8 = nullable.Value + MoveDimText * scale;
                    nullable = dimensionSegmentList.Last().Value;
                    double num9 = nullable.Value;
                    double num10 = (num8 + num9) * DirectionX;
                    XYZ xyz1 = new XYZ(x1 + num10, ((IEnumerable<DimensionSegment>)dimensionSegmentList).Last().TextPosition.Y, ((IEnumerable<DimensionSegment>)dimensionSegmentList).Last().TextPosition.Z);
                    dimensionSegment1.TextPosition = xyz1;
                }
                else
                {
                    nullable = dimensionSegmentList[0].Value;
                    double num3 = scaleMm;
                    if (nullable.HasValue && nullable.GetValueOrDefault() < num3)
                    {
                        DimensionSegment dimensionSegment = dimensionSegmentList[0];
                        double x = dimensionSegmentList[0].TextPosition.X;
                        double y = dimensionSegmentList[0].TextPosition.Y;
                        nullable = dimensionSegmentList[0].Value;
                        double num4 = nullable.Value + MoveDimText * scale;
                        nullable = dimensionSegmentList[0].Value;
                        double num5 = nullable.Value;
                        double num6 = (num4 + num5) * DirectionY;
                        double num7 = y - num6;
                        double z = dimensionSegmentList[0].TextPosition.Z;
                        XYZ xyz = new XYZ(x, num7, z);
                        dimensionSegment.TextPosition = xyz;
                    }
                    nullable = ((IEnumerable<DimensionSegment>)dimensionSegmentList).Last().Value;
                    double num8 = scaleMm;
                    if (nullable.HasValue && nullable.GetValueOrDefault() < num8)
                    {
                        DimensionSegment dimensionSegment = ((IEnumerable<DimensionSegment>)dimensionSegmentList).Last();
                        double x = ((IEnumerable<DimensionSegment>)dimensionSegmentList).Last().TextPosition.X;
                        double y = ((IEnumerable<DimensionSegment>)dimensionSegmentList).Last().TextPosition.Y;
                        nullable = ((IEnumerable<DimensionSegment>)dimensionSegmentList).Last().Value;
                        double num4 = nullable.Value + MoveDimText * scale;
                        nullable = ((IEnumerable<DimensionSegment>)dimensionSegmentList).Last().Value;
                        double num5 = nullable.Value;
                        double num6 = (num4 + num5) * DirectionY;
                        double num7 = y + num6;
                        double z = ((IEnumerable<DimensionSegment>)dimensionSegmentList).Last().TextPosition.Z;
                        XYZ xyz = new XYZ(x, num7, z);
                        dimensionSegment.TextPosition = xyz;
                    }
                }
            }
            else
            {
                double? nullable = dim.Value;
                double num3 = scaleMm;
                if (nullable.HasValue && nullable.GetValueOrDefault() < num3)
                {
                    Dimension dimension = dim;
                    double x = dim.TextPosition.X;
                    nullable = dim.Value;
                    double num4 = nullable.Value;
                    double moveDimText = MoveDimText;
                    nullable = dim.Value;
                    double num5 = nullable.Value;
                    double num6 = moveDimText + num5;
                    double num7 = (num4 + num6) * DirectionX;
                    double num8 = x + num7;
                    double y = dim.TextPosition.Y;
                    nullable = dim.Value;
                    double num9 = nullable.Value + MoveDimText;
                    nullable = dim.Value;
                    double num10 = nullable.Value;
                    double num11 = (num9 + num10) * DirectionY;
                    double num12 = y + num11;
                    double num13 = 0.0;
                    XYZ xyz = new XYZ(num8, num12, num13);
                    dimension.TextPosition = xyz;
                }
            }
        }


        /// <summary>
        /// Converts a value from milimeters to feet
        /// </summary>
        /// <param name="ft"></param>
        /// <returns></returns>
        public static double MmToFt(double ft)
        {
            return ft / 304.8;
        }

        /// <summary>
        /// Converts a value from feet to milimeters
        /// </summary>
        /// <param name="len"></param>
        /// <returns></returns>
        public static int FtTomm(double len)
        {
            return Convert.ToInt32(Math.Round(len * 304.8 / 10.0) * 10.0);
        }

        public static string RealString(double value)
        {
            return value.ToString("0.##");
        }
    }



    /// <summary>
    /// Retrieve the error message for displaying in the Revit interface
    /// </summary>
    public class ErrorMessageException : ApplicationException
    {
        /// <summary>
        /// constructor entirely using baseclass
        /// </summary>
        public ErrorMessageException()
            : base()
        {
        }

        public ErrorMessageException(String message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Manage Warning in the Revit interface
    /// </summary>
    public class PrintPreprocessor : IFailuresPreprocessor
    {
        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            // Inside event handler, get all warnings
            IList<FailureMessageAccessor> failList = failuresAccessor.GetFailureMessages();
            foreach (FailureMessageAccessor failure in failList)
            {
                // check FailureDefinitionIds against ones that you want to dismiss
                FailureDefinitionId failId = failure.GetFailureDefinitionId();
                // prevent Revit from showing Unenclosed room warnings
                if (failId == BuiltInFailures.OverlapFailures.WallsOverlap)
                {
                    failuresAccessor.DeleteWarning(failure);
                }
            }
            return FailureProcessingResult.Continue;
        }

    }
}
