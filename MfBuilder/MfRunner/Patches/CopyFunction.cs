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
        public static void CopyFunction(IntPtr Address, byte[] Data)
        {
            Marshal.Copy(Data, 0, Address, Data.Length);
        }
    }
}
