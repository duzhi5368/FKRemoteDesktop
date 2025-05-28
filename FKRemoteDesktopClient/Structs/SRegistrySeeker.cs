using FKRemoteDesktop.Extensions;
using FKRemoteDesktop.Helpers;
using FKRemoteDesktop.Message.MessageStructs;
using Microsoft.Win32;
using System;
using System.Collections.Generic;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Structs
{
    public class SRegistrySeeker
    {
        private readonly List<RegSeekerMatch> _matches;

        public RegSeekerMatch[] Matches => _matches?.ToArray();

        public SRegistrySeeker()
        {
            _matches = new List<RegSeekerMatch>();
        }

        public void BeginSeeking(string rootKeyName)
        {
            if (!String.IsNullOrEmpty(rootKeyName))
            {
                using (RegistryKey root = GetRootKey(rootKeyName))
                {
                    if (root != null && root.Name != rootKeyName)
                    {
                        string subKeyName = rootKeyName.Substring(root.Name.Length + 1);
                        using (RegistryKey subroot = root.OpenReadonlySubKeySafe(subKeyName))
                        {
                            if (subroot != null)
                                Seek(subroot);
                        }
                    }
                    else
                    {
                        Seek(root);
                    }
                }
            }
            else
            {
                Seek(null);
            }
        }

        private void Seek(RegistryKey rootKey)
        {
            if (rootKey == null)
            {
                foreach (RegistryKey key in GetRootKeys())
                    ProcessKey(key, key.Name);
            }
            else
            {
                Search(rootKey);
            }
        }

        private void Search(RegistryKey rootKey)
        {
            foreach (string subKeyName in rootKey.GetSubKeyNames())
            {
                RegistryKey subKey = rootKey.OpenReadonlySubKeySafe(subKeyName);
                ProcessKey(subKey, subKeyName);
            }
        }

        private void ProcessKey(RegistryKey key, string keyName)
        {
            if (key != null)
            {
                List<RegValueData> values = new List<RegValueData>();
                foreach (string valueName in key.GetValueNames())
                {
                    RegistryValueKind valueType = key.GetValueKind(valueName);
                    object valueData = key.GetValue(valueName);
                    values.Add(RegistryKeyHelper.CreateRegValueData(valueName, valueType, valueData));
                }
                AddMatch(keyName, RegistryKeyHelper.AddDefaultValue(values), key.SubKeyCount);
            }
            else
            {
                AddMatch(keyName, RegistryKeyHelper.GetDefaultValues(), 0);
            }
        }

        private void AddMatch(string key, RegValueData[] values, int subkeycount)
        {
            RegSeekerMatch match = new RegSeekerMatch { Key = key, Data = values, HasSubKeys = subkeycount > 0 };
            _matches.Add(match);
        }

        public static RegistryKey GetRootKey(string subkeyFullPath)
        {
            string[] path = subkeyFullPath.Split('\\');
            try
            {
                switch (path[0])
                {
                    case "HKEY_CLASSES_ROOT":
                        return RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Registry64);

                    case "HKEY_CURRENT_USER":
                        return RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);

                    case "HKEY_LOCAL_MACHINE":
                        return RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);

                    case "HKEY_USERS":
                        return RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView.Registry64);

                    case "HKEY_CURRENT_CONFIG":
                        return RegistryKey.OpenBaseKey(RegistryHive.CurrentConfig, RegistryView.Registry64);

                    default:
                        throw new Exception("根注册表项无效.");
                }
            }
            catch (SystemException)
            {
                throw new Exception("缺失权限，无法打开根注册表项.");
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static List<RegistryKey> GetRootKeys()
        {
            List<RegistryKey> rootKeys = new List<RegistryKey>();
            try
            {
                rootKeys.Add(RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Registry64));
                rootKeys.Add(RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64));
                rootKeys.Add(RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64));
                rootKeys.Add(RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView.Registry64));
                rootKeys.Add(RegistryKey.OpenBaseKey(RegistryHive.CurrentConfig, RegistryView.Registry64));
            }
            catch (SystemException)
            {
                throw new Exception("缺失权限，无法打开根注册表项.");
            }
            catch (Exception e)
            {
                throw e;
            }
            return rootKeys;
        }
    }
}