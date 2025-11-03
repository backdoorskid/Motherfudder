using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MfRunner
{
    internal partial class Program
    {
#if SINGLE_INSTANCE
        public static Mutex SingleInstanceMutex;
        public static void CloseIfAlreadyRunning()
        {
            SingleInstanceMutex = new Mutex(false, SINGLE_INSTANCE_MUTEX);
            try
            {
                if (!SingleInstanceMutex.WaitOne(TimeSpan.FromSeconds(5), false))
                {
                    Environment.Exit(0);
                }
            }
            catch (AbandonedMutexException) { }
        }
#endif
    }
}
