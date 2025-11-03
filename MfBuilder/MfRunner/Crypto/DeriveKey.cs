using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MfRunner
{
    internal partial class Program
    {
        public static byte[] DeriveKey(byte seed)
        {
            var keyBytesCloned = (byte[])DERIVE_KEY_STARTING_STATE.Clone();

            for (int i = 0; i < keyBytesCloned.Length; i++) {
                keyBytesCloned[i] += FuckShitUp;
                keyBytesCloned[i] ^= seed;
            }

            return SHA256.Create().ComputeHash(keyBytesCloned);
        }
    }
}
