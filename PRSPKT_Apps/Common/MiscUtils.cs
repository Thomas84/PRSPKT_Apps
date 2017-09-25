using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRSPKT_Apps.Common
{
    public static class MiscUtils
    {
        public static double MillimetersToFeet(double lengthInMM)
        {
            return lengthInMM / 304.8;
        }

        public static double FeetToMillimeters(double lengthInFeet)
        {
            return lengthInFeet * 304.8;
        }
    }
}
