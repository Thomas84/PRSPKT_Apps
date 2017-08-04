using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace PRSPKT_Apps
{
    class Tools
    {
        // Define CultureInfo
        public static ResourceManager LangResMan;
        public static CultureInfo Cult;

        public static void GetLocalisationValues()
        {
            Tools.Cult = CultureInfo.CreateSpecificCulture("ru");
            Tools.LangResMan = new ResourceManager("PRSPKT_Apps.Resources.ru", System.Reflection.Assembly.GetExecutingAssembly());
        }

        public static double? GetValueFromString(string text, Units units)
        {
            // Check the string value
            string heightValueString = text;
            double length;

            if (UnitFormatUtils.TryParse(units, UnitType.UT_Length, heightValueString, out length))
            {
                return length;
            }
            else return null;
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
