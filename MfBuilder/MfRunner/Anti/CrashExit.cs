using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MfRunner
{
    internal partial class Program
    {
        public static byte FuckShitUp = 0;

        public static void CrashExit()
        {
            FuckShitUp = 1; // If these two below functions somehow don't work, we can at least mess up execution.
            CustomRaiseException(0, 0, 0, IntPtr.Zero);
            Environment.Exit(0);
        }
    }
}
