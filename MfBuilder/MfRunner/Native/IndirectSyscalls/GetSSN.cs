using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MfRunner
{
#if NATIVE
    internal partial class Program
    {
        static byte[] NtdllBytes;
        public static short GetNtdllSSN(uint ExportNameH)
        {
            if (NtdllBytes == null)
            {
                NtdllBytes = File.ReadAllBytes("C:\\Windows\\System32\\ntdll.dll");
            }

            IntPtr exportAddr = GetExportAddress(NtdllAddress, ExportNameH);
            long offset = ((long)exportAddr) - NtdllAddress;

#if X64
            short syscallNumber = (short)((NtdllBytes[offset + 5] << 8) | NtdllBytes[offset + 4]);
#else
            offset -= 0x0C00;
            short syscallNumber = (short)((NtdllBytes[offset + 2] << 8) | NtdllBytes[offset + 1]);
#endif
            return syscallNumber;
        }
    }
#endif
}
