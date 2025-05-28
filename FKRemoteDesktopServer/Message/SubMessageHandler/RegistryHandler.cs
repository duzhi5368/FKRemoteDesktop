using FKRemoteDesktop.Message.MessageStructs;
using FKRemoteDesktop.Message.SubMessages;
using FKRemoteDesktop.Network;
using Microsoft.Win32;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message.SubMessageHandler
{
    public class RegistryHandler : MessageProcessorBase<string>
    {
        private readonly Client _client;

        public delegate void KeysReceivedEventHandler(object sender, string rootKey, RegSeekerMatch[] matches);
        public delegate void KeyCreatedEventHandler(object sender, string parentPath, RegSeekerMatch match);
        public delegate void KeyDeletedEventHandler(object sender, string parentPath, string subKey);
        public delegate void KeyRenamedEventHandler(object sender, string parentPath, string oldSubKey, string newSubKey);
        public delegate void ValueCreatedEventHandler(object sender, string keyPath, RegValueData value);
        public delegate void ValueDeletedEventHandler(object sender, string keyPath, string valueName);
        public delegate void ValueRenamedEventHandler(object sender, string keyPath, string oldValueName, string newValueName);
        public delegate void ValueChangedEventHandler(object sender, string keyPath, RegValueData value);

        public event KeysReceivedEventHandler KeysReceived;
        public event KeyCreatedEventHandler KeyCreated;
        public event KeyDeletedEventHandler KeyDeleted;
        public event KeyRenamedEventHandler KeyRenamed;
        public event ValueCreatedEventHandler ValueCreated;
        public event ValueDeletedEventHandler ValueDeleted;
        public event ValueRenamedEventHandler ValueRenamed;
        public event ValueChangedEventHandler ValueChanged;

        private void OnKeysReceived(string rootKey, RegSeekerMatch[] matches)
        {
            SynchronizationContext.Post(t =>
            {
                var handler = KeysReceived;
                handler?.Invoke(this, rootKey, (RegSeekerMatch[])t);
            }, matches);
        }

        private void OnKeyCreated(string parentPath, RegSeekerMatch match)
        {
            SynchronizationContext.Post(t =>
            {
                var handler = KeyCreated;
                handler?.Invoke(this, parentPath, (RegSeekerMatch)t);
            }, match);
        }

        private void OnKeyDeleted(string parentPath, string subKey)
        {
            SynchronizationContext.Post(t =>
            {
                var handler = KeyDeleted;
                handler?.Invoke(this, parentPath, (string)t);
            }, subKey);
        }

        private void OnKeyRenamed(string parentPath, string oldSubKey, string newSubKey)
        {
            SynchronizationContext.Post(t =>
            {
                var handler = KeyRenamed;
                handler?.Invoke(this, parentPath, oldSubKey, (string)t);
            }, newSubKey);
        }

        private void OnValueCreated(string keyPath, RegValueData value)
        {
            SynchronizationContext.Post(t =>
            {
                var handler = ValueCreated;
                handler?.Invoke(this, keyPath, (RegValueData)t);
            }, value);
        }

        private void OnValueDeleted(string keyPath, string valueName)
        {
            SynchronizationContext.Post(t =>
            {
                var handler = ValueDeleted;
                handler?.Invoke(this, keyPath, (string)t);
            }, valueName);
        }

        private void OnValueRenamed(string keyPath, string oldValueName, string newValueName)
        {
            SynchronizationContext.Post(t =>
            {
                var handler = ValueRenamed;
                handler?.Invoke(this, keyPath, oldValueName, (string)t);
            }, newValueName);
        }

        private void OnValueChanged(string keyPath, RegValueData value)
        {
            SynchronizationContext.Post(t =>
            {
                var handler = ValueChanged;
                handler?.Invoke(this, keyPath, (RegValueData)t);
            }, value);
        }

        public RegistryHandler(Client client) : base(true)
        {
            _client = client;
        }

        public override bool CanExecute(IMessage message) => message is GetRegistryKeysResponse ||
                                                     message is GetCreateRegistryKeyResponse ||
                                                     message is GetDeleteRegistryKeyResponse ||
                                                     message is GetRenameRegistryKeyResponse ||
                                                     message is GetCreateRegistryValueResponse ||
                                                     message is GetDeleteRegistryValueResponse ||
                                                     message is GetRenameRegistryValueResponse ||
                                                     message is GetChangeRegistryValueResponse;

        public override bool CanExecuteFrom(ISender sender) => _client.Equals(sender);

        public override void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case GetRegistryKeysResponse keysResp:
                    Execute(sender, keysResp);
                    break;
                case GetCreateRegistryKeyResponse createKeysResp:
                    Execute(sender, createKeysResp);
                    break;
                case GetDeleteRegistryKeyResponse deleteKeysResp:
                    Execute(sender, deleteKeysResp);
                    break;
                case GetRenameRegistryKeyResponse renameKeysResp:
                    Execute(sender, renameKeysResp);
                    break;
                case GetCreateRegistryValueResponse createValueResp:
                    Execute(sender, createValueResp);
                    break;
                case GetDeleteRegistryValueResponse deleteValueResp:
                    Execute(sender, deleteValueResp);
                    break;
                case GetRenameRegistryValueResponse renameValueResp:
                    Execute(sender, renameValueResp);
                    break;
                case GetChangeRegistryValueResponse changeValueResp:
                    Execute(sender, changeValueResp);
                    break;
            }
        }

        public void LoadRegistryKey(string rootKeyName)
        {
            _client.Send(new DoLoadRegistryKey
            {
                RootKeyName = rootKeyName
            });
        }

        public void CreateRegistryKey(string parentPath)
        {
            _client.Send(new DoCreateRegistryKey
            {
                ParentPath = parentPath
            });
        }

        public void DeleteRegistryKey(string parentPath, string keyName)
        {
            _client.Send(new DoDeleteRegistryKey
            {
                ParentPath = parentPath,
                KeyName = keyName
            });
        }

        public void RenameRegistryKey(string parentPath, string oldKeyName, string newKeyName)
        {
            _client.Send(new DoRenameRegistryKey
            {
                ParentPath = parentPath,
                OldKeyName = oldKeyName,
                NewKeyName = newKeyName
            });
        }

        public void CreateRegistryValue(string keyPath, RegistryValueKind kind)
        {
            _client.Send(new DoCreateRegistryValue
            {
                KeyPath = keyPath,
                Kind = kind
            });
        }

        public void DeleteRegistryValue(string keyPath, string valueName)
        {
            _client.Send(new DoDeleteRegistryValue
            {
                KeyPath = keyPath,
                ValueName = valueName
            });
        }

        public void RenameRegistryValue(string keyPath, string oldValueName, string newValueName)
        {
            _client.Send(new DoRenameRegistryValue
            {
                KeyPath = keyPath,
                OldValueName = oldValueName,
                NewValueName = newValueName
            });
        }

        public void ChangeRegistryValue(string keyPath, RegValueData value)
        {
            _client.Send(new DoChangeRegistryValue
            {
                KeyPath = keyPath,
                Value = value
            });
        }

        private void Execute(ISender client, GetRegistryKeysResponse message)
        {
            if (!message.IsError)
            {
                OnKeysReceived(message.RootKey, message.Matches);
            }
            else
            {
                OnReport(message.ErrorMsg);
            }
        }

        private void Execute(ISender client, GetCreateRegistryKeyResponse message)
        {
            if (!message.IsError)
            {
                OnKeyCreated(message.ParentPath, message.Match);
            }
            else
            {
                OnReport(message.ErrorMsg);
            }
        }

        private void Execute(ISender client, GetDeleteRegistryKeyResponse message)
        {
            if (!message.IsError)
            {
                OnKeyDeleted(message.ParentPath, message.KeyName);
            }
            else
            {
                OnReport(message.ErrorMsg);
            }
        }

        private void Execute(ISender client, GetRenameRegistryKeyResponse message)
        {
            if (!message.IsError)
            {
                OnKeyRenamed(message.ParentPath, message.OldKeyName, message.NewKeyName);
            }
            else
            {
                OnReport(message.ErrorMsg);
            }
        }

        private void Execute(ISender client, GetCreateRegistryValueResponse message)
        {
            if (!message.IsError)
            {
                OnValueCreated(message.KeyPath, message.Value);
            }
            else
            {
                OnReport(message.ErrorMsg);
            }
        }

        private void Execute(ISender client, GetDeleteRegistryValueResponse message)
        {
            if (!message.IsError)
            {
                OnValueDeleted(message.KeyPath, message.ValueName);
            }
            else
            {
                OnReport(message.ErrorMsg);
            }
        }

        private void Execute(ISender client, GetRenameRegistryValueResponse message)
        {
            if (!message.IsError)
            {
                OnValueRenamed(message.KeyPath, message.OldValueName, message.NewValueName);
            }
            else
            {
                OnReport(message.ErrorMsg);
            }
        }

        private void Execute(ISender client, GetChangeRegistryValueResponse message)
        {
            if (!message.IsError)
            {
                OnValueChanged(message.KeyPath, message.Value);
            }
            else
            {
                OnReport(message.ErrorMsg);
            }
        }
    }
}
