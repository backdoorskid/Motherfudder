using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MfRunner
{
    internal partial class Program
    {
#if ANTI_VM
        public static void CrashIfVm()
        {
            using (var search = new ManagementObjectSearcher("Select * from Win32_ComputerSystem"))
            {
                using (var things = search.Get())
                {
                    foreach (var thing in things)
                    {
                        string manufacturer = thing["Manufacturer"].ToString().ToLower();
                        string model        = thing["Model"].ToString().ToLower();
                        if ((manufacturer == "microsoft corporation" && model.Contains("virtual")) || manufacturer.Contains("vmware") || model == "virtualbox")
                        {
                            FuckShitUp = 5;
                            CrashExit();
                        }
                    }
                }
            }
        }
#endif
    }
}
