using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace MfObfDotNet
{
    public partial class Random
    {
        public static RandomNumberGenerator rng = RandomNumberGenerator.Create();
        
        public static uint U32()
        {
            var b = new byte[4];
            rng.GetBytes(b);
            return BitConverter.ToUInt32(b, 0);
        }

        public static ulong U64()
        {
            var b = new byte[8];
            rng.GetBytes(b);
            return BitConverter.ToUInt64(b, 0);
        }

        public static void Shuffle<T>(List<T> Sequence)
        {
            for (int s = 0; s < Sequence.Count - 1; s++)
            {
                int GenObj = (int)(s + (U32() % (Sequence.Count - s))); //GenerateAnotherNum(s, Sequence.Length);

                var h = Sequence[s];
                Sequence[s] = Sequence[GenObj];
                Sequence[GenObj] = h;
            }
        }

        public static string String()
        {
            string r = "";
            string c = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            for (int i = 0; i < 10 + (U32() % 11); i++)
            {
                r += c[(int)(U32() % 62)];
            }
            return r;
        }
    }
}
