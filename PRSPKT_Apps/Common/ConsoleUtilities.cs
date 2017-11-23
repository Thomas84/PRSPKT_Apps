using System.Diagnostics;
using System.Security;
using System.Security.Permissions;

namespace PRSPKT_Apps.Common
{
    internal static class ConsoleUtilities
    {
        [SecurityCritical]
        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]

        internal static void StartHiddenConsoleProg(string exePath, string args)
        {
            StartHiddenConsoleProg(exePath, args, 20000);
        }

        [SecurityCritical]
        [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")]
        internal static void StartHiddenConsoleProg(string exePath, string args, int waitTime)
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = exePath
            };

            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            if (!string.IsNullOrEmpty(args))
            {
                startInfo.Arguments = args;
            }

            Process p = Process.Start(startInfo);
            p.WaitForExit(waitTime);
            p.Dispose();
        }
    }
}
