using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRSPKT_Apps.ApartmentCalc_P
{
    public class HelpMe
    {
        private static string _currentVersion;
        public static string CurrentVersion { get => _currentVersion; set => _currentVersion = value; }

        public static string GetVersion()
        {
            _currentVersion = "0.66b";
            return "ВЕРСИЯ " + _currentVersion;
        }
    }
}
