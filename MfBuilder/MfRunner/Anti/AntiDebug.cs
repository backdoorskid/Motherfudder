using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MfRunner
{
    internal partial class Program
    {
#if ANTI_DEBUG
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool IsDebuggerPresentType();

        public static void StartDebuggerCheckThread()
        {
            new Thread(() => {
                IntPtr IsDebuggerPresentAddress = GetExportAddress(Kernel32Address, 0x26B4BD03); // IsDebuggerPresent
                IsDebuggerPresentType IsDebuggerPresent = (IsDebuggerPresentType)Marshal.GetDelegateForFunctionPointer(IsDebuggerPresentAddress, typeof(IsDebuggerPresentType));

                while (true)
                {
                    if (IsDebuggerPresent() || Debugger.IsAttached) { FuckShitUp = 2; CrashExit(); }
                    Thread.Sleep(5000);
                }
            }).Start();
        }
#endif
    }
}
