using System.IO;
using System.Windows.Forms;
using System.Xml.XPath;
using System.Xml;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Configs
{
    public static class ServerConfig
    {
        private static readonly string SettingsPath = Path.Combine(Application.StartupPath, "FKRemoteDesktop.xml");
        public static readonly string CertificatePath = Path.Combine(Application.StartupPath, "FKRemoteDesktop.p12");

        #region 内部函数
        private static string ReadValue(string pstrValueToRead)
        {
            try
            {
                XPathDocument doc = new XPathDocument(SettingsPath);
                XPathNavigator nav = doc.CreateNavigator();
                XPathExpression expr = nav.Compile(@"/settings/" + pstrValueToRead);
                XPathNodeIterator iterator = nav.Select(expr);
                while (iterator.MoveNext())
                {
                    return iterator.Current.Value;
                }
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string ReadValueSafe(string pstrValueToRead, string defaultValue = "")
        {
            string value = ReadValue(pstrValueToRead);
            return (!string.IsNullOrEmpty(value)) ? value : defaultValue;
        }

        private static void WriteValue(string pstrValueToRead, string pstrValueToWrite)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                if (File.Exists(SettingsPath))
                {
                    using (var reader = new XmlTextReader(SettingsPath))
                    {
                        doc.Load(reader);
                    }
                }
                else
                {
                    var dir = Path.GetDirectoryName(SettingsPath);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    doc.AppendChild(doc.CreateElement("settings"));
                }

                XmlElement root = doc.DocumentElement;
                XmlNode oldNode = root.SelectSingleNode(@"/settings/" + pstrValueToRead);
                if (oldNode == null) // 如不存在，则手动创建
                {
                    oldNode = doc.SelectSingleNode("settings");
                    oldNode.AppendChild(doc.CreateElement(pstrValueToRead)).InnerText = pstrValueToWrite;
                    doc.Save(SettingsPath);
                    return;
                }
                oldNode.InnerText = pstrValueToWrite;
                doc.Save(SettingsPath);
            }
            catch
            {
            }
        }
        #endregion

        public static ushort ListenPort
        {
            get { return ushort.Parse(ReadValueSafe("ListenPort", "49663")); }
            set { WriteValue("ListenPort", value.ToString()); }
        }

        public static bool IPv6Support
        {
            get{ return bool.Parse(ReadValueSafe("IPv6Support", "False")); }
            set{ WriteValue("IPv6Support", value.ToString()); }
        }

        public static bool AutoListen
        {
            get{ return bool.Parse(ReadValueSafe("AutoListen", "True"));}
            set{ WriteValue("AutoListen", value.ToString()); }
        }

        public static bool UseUPnP
        {
            get{ return bool.Parse(ReadValueSafe("UseUPnP", "False")); }
            set{ WriteValue("UseUPnP", value.ToString()); }
        }

        public static string SaveFormat
        {
            get{ return ReadValueSafe("SaveFormat", "[APP:KEY] - URL  - USER:PASS"); }
            set{ WriteValue("SaveFormat", value); }
        }

        public static ushort ReverseProxyPort
        {
            get{ return ushort.Parse(ReadValueSafe("ReverseProxyPort", "7681")); }
            set{ WriteValue("ReverseProxyPort", value.ToString()); }
        }
    }
}
