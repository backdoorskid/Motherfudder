using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MfRunner
{
    internal partial class Program
    {
        public static uint HashString(string s)
        {
            uint r = 0x41414141;
            foreach (char c in s.ToLower())
            {
                // R = ROTL(R * 9 + C, 9);

                r += (r << 3) + r + c;
                r =  (r << 9) | (r >> 23);
            }
            return r;
        }
    }
}
