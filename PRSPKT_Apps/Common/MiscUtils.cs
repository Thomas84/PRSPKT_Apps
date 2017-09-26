using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRSPKT_Apps.Common
{
    public static class MiscUtils
    {
        public static string GetDateString
        {
            get
            {
                DateTime moment = DateTime.Now;
                string year = moment.Year.ToString(CultureInfo.CurrentCulture);
                string month = PadLeftZeros(moment.Month.ToString(CultureInfo.CurrentCulture), 2);
                string day = PadLeftZeros(moment.Day.ToString(CultureInfo.CurrentCulture), 2);
                return year + month + day;
            }
        }

        private static string PadLeftZeros(string v1, int v2)
        {
            if (string.IsNullOrEmpty(v1))
            {
                return string.Empty;
            }
            if (v2 > 1 && v1.Length == v2 -1)
            {
                return "0" + v1;
            }
            return v1;
        }

        public static double MillimetersToFeet(double lengthInMM)
        {
            return lengthInMM / 304.8;
        }
        /// <summary>
        /// Convert from feet to millimeters by * 304.8
        /// </summary>
        /// <param name="lengthInFeet"></param>
        /// <returns></returns>
        public static double FeetToMillimeters(double lengthInFeet)
        {
            return lengthInFeet * 304.8;
        }

        public static string GetNiceViewName(Document doc, string request)
        {
            if (ViewNameIsAvailable(doc, request))
            {
                return request;
            }
            else
            {
                return request + @"(" + (DateTime.Now.TimeOfDay.Ticks / 100000).ToString(CultureInfo.InvariantCulture) + @")";
            }
        }

        private static bool ViewNameIsAvailable(Document doc, string request)
        {
            using (var fec = new FilteredElementCollector(doc))
            {
                fec.OfClass(typeof(Autodesk.Revit.DB.View));
                foreach (View view in fec)
                {
                    var v = view as View;
                    if (v.ViewName == request)
                    {
                        return false;
                    }
                }
            }
            return true;
        }



    }

}
