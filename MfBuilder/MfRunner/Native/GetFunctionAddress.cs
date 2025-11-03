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
        public static IntPtr GetLoadedModuleAddress(uint DLLNameH)
        {
            ProcessModuleCollection ProcModules = Process.GetCurrentProcess().Modules;
            foreach (ProcessModule Mod in ProcModules)
            {
                if (DLLNameH == HashString(Mod.FileName.Split('\\').Last()))
                {
                    return Mod.BaseAddress;
                }
            }
            return IntPtr.Zero;
        }
        public static IntPtr GetExportAddress(long ModuleBase, uint ExportNameH)
        {
            IntPtr FunctionPtr = IntPtr.Zero;
            try
            {
                int PeHeader = Marshal.ReadInt32((IntPtr)(ModuleBase + 0x3C));
                short OptHeaderSize = Marshal.ReadInt16((IntPtr)(ModuleBase + PeHeader + 0x14));
                long OptHeader = ModuleBase + PeHeader + 0x18;

                int ExportRVA = Marshal.ReadInt32((IntPtr)(OptHeader + ((Marshal.ReadInt16((IntPtr)OptHeader) == 0x010b) ? 0x60 : 0x70)));
                int OrdinalBase = Marshal.ReadInt32((IntPtr)(ModuleBase + ExportRVA + 0x10));
                int NumberOfFunctions = Marshal.ReadInt32((IntPtr)(ModuleBase + ExportRVA + 0x14));
                int NumberOfNames = Marshal.ReadInt32((IntPtr)(ModuleBase + ExportRVA + 0x18));
                int FunctionsRVA = Marshal.ReadInt32((IntPtr)(ModuleBase + ExportRVA + 0x1C));
                int NamesRVA = Marshal.ReadInt32((IntPtr)(ModuleBase + ExportRVA + 0x20));
                int OrdinalsRVA = Marshal.ReadInt32((IntPtr)(ModuleBase + ExportRVA + 0x24));

                for (int i = 0; i < NumberOfNames; i++)
                {
                    uint FunctionHash = HashString(Marshal.PtrToStringAnsi((IntPtr)(ModuleBase + Marshal.ReadInt32((IntPtr)(ModuleBase + NamesRVA + i * 4)))));
                    if (FunctionHash == ExportNameH)
                    {
                        int FunctionOrdinal = Marshal.ReadInt16((IntPtr)(ModuleBase + OrdinalsRVA + i * 2)) + OrdinalBase;
                        int FunctionRVA = Marshal.ReadInt32((IntPtr)(ModuleBase + FunctionsRVA + (4 * (FunctionOrdinal - OrdinalBase))));
                        FunctionPtr = (IntPtr)(ModuleBase + FunctionRVA);
                        break;
                    }
                }
            }
            catch
            {
                return IntPtr.Zero;
            }

            return FunctionPtr;
        }
        public static IntPtr GetLibraryAddress(uint DLLNameH, uint FunctionNameH)
        {
            long hModule = (long)GetLoadedModuleAddress(DLLNameH);
            if (hModule == 0)
            {
                return IntPtr.Zero;
            }

            return GetExportAddress(hModule, FunctionNameH);
        }
    }
}
