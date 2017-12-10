using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRSPKT_Apps.ApartmentCalc_P
{
    public class HelpMe
    {
        private static readonly string _currentVersion = "0.68b";

        public static string GetVersion()
        {
            return "ВЕРСИЯ " + _currentVersion;
        }
    }
}
