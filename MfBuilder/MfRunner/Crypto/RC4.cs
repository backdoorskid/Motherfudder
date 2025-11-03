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
        public static void ApplyRC4(byte[] data, byte[] key)
        {
            byte i, j, k;
            i = 0;
            j = 0;
            k = 0;

            byte[] s = new byte[256];
            for (int _ = 0; _ < 256; _++)
                s[_] = (byte)_;

            do {
                j += s[i];
                j += key[i % key.Length];
                k = s[i];
                s[i] = s[j];
                s[j] = k;
                i++;
            } while (i != 0);

            i = j = 0;
            for (int _ = 0; _ < data.Length; _++)
            {
                i++;
                j += s[i];
                k = s[i];
                s[i] = s[j];
                s[j] = k;
                data[_] ^= s[(s[i] + s[j]) % 256];
            }
        }
    }
}
