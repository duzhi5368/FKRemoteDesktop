using FKRemoteDesktop.Utilities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Extensions
{
    public static class RegistryKeyExtensions
    {
        private static bool IsNameOrValueNull(this string keyName, RegistryKey key)
        {
            return (string.IsNullOrEmpty(keyName) || (key == null));
        }

        public static string GetValueSafe(this RegistryKey key, string keyName, string defaultValue = "")
        {
            try
            {
                return key.GetValue(keyName, defaultValue).ToString();
            }
            catch
            {
                return defaultValue;
            }
        }

        public static RegistryKey OpenReadonlySubKeySafe(this RegistryKey key, string name)
        {
            try
            {
                return key.OpenSubKey(name, false);
            }
            catch
            {
                return null;
            }
        }

        public static RegistryKey OpenWritableSubKeySafe(this RegistryKey key, string name)
        {
            try
            {
                return key.OpenSubKey(name, true);
            }
            catch
            {
                return null;
            }
        }

        public static RegistryKey CreateSubKeySafe(this RegistryKey key, string name)
        {
            try
            {
                return key.CreateSubKey(name);
            }
            catch
            {
                return null;
            }
        }

        public static bool DeleteSubKeyTreeSafe(this RegistryKey key, string name)
        {
            try
            {
                key.DeleteSubKeyTree(name, true);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool RenameSubKeySafe(this RegistryKey key, string oldName, string newName)
        {
            try
            {
                key.CopyKey(oldName, newName);
                key.DeleteSubKeyTree(oldName);
                return true;
            }
            catch
            {
                key.DeleteSubKeyTreeSafe(newName);
                return false;
            }
        }

        public static void CopyKey(this RegistryKey key, string oldName, string newName)
        {
            using (RegistryKey newKey = key.CreateSubKey(newName))
            {
                using (RegistryKey oldKey = key.OpenSubKey(oldName, true))
                {
                    RecursiveCopyKey(oldKey, newKey);
                }
            }
        }

        private static void RecursiveCopyKey(RegistryKey sourceKey, RegistryKey destKey)
        {
            foreach (string valueName in sourceKey.GetValueNames())
            {
                object valueObj = sourceKey.GetValue(valueName);
                RegistryValueKind valueKind = sourceKey.GetValueKind(valueName);
                destKey.SetValue(valueName, valueObj, valueKind);
            }
            foreach (string subKeyName in sourceKey.GetSubKeyNames())
            {
                using (RegistryKey sourceSubkey = sourceKey.OpenSubKey(subKeyName))
                {
                    using (RegistryKey destSubKey = destKey.CreateSubKey(subKeyName))
                    {
                        RecursiveCopyKey(sourceSubkey, destSubKey);
                    }
                }
            }
        }

        public static bool SetValueSafe(this RegistryKey key, string name, object data, RegistryValueKind kind)
        {
            try
            {
                if (kind != RegistryValueKind.Binary && data.GetType() == typeof(byte[]))
                {
                    switch (kind)
                    {
                        case RegistryValueKind.String:
                        case RegistryValueKind.ExpandString:
                            data = ByteConverter.ToString((byte[])data);
                            break;

                        case RegistryValueKind.DWord:
                            data = ByteConverter.ToUInt32((byte[])data);
                            break;

                        case RegistryValueKind.QWord:
                            data = ByteConverter.ToUInt64((byte[])data);
                            break;

                        case RegistryValueKind.MultiString:
                            data = ByteConverter.ToStringArray((byte[])data);
                            break;
                    }
                }
                key.SetValue(name, data, kind);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool DeleteValueSafe(this RegistryKey key, string name)
        {
            try
            {
                key.DeleteValue(name);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool RenameValueSafe(this RegistryKey key, string oldName, string newName)
        {
            try
            {
                key.CopyValue(oldName, newName);
                key.DeleteValue(oldName);
                return true;
            }
            catch
            {
                key.DeleteValueSafe(newName);
                return false;
            }
        }

        public static void CopyValue(this RegistryKey key, string oldName, string newName)
        {
            RegistryValueKind valueKind = key.GetValueKind(oldName);
            object valueData = key.GetValue(oldName);
            key.SetValue(newName, valueData, valueKind);
        }

        public static bool ContainsSubKey(this RegistryKey key, string name)
        {
            foreach (string subkey in key.GetSubKeyNames())
            {
                if (subkey == name)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool ContainsValue(this RegistryKey key, string name)
        {
            foreach (string value in key.GetValueNames())
            {
                if (value == name)
                {
                    return true;
                }
            }
            return false;
        }

        public static IEnumerable<Tuple<string, string>> GetKeyValues(this RegistryKey key)
        {
            if (key == null)
                yield break;
            foreach (var k in key.GetValueNames().Where(keyVal => !keyVal.IsNameOrValueNull(key)).Where(k => !string.IsNullOrEmpty(k)))
            {
                yield return new Tuple<string, string>(k, key.GetValueSafe(k));
            }
        }

        public static object GetDefault(this RegistryValueKind valueKind)
        {
            switch (valueKind)
            {
                case RegistryValueKind.Binary:
                    return new byte[] { };

                case RegistryValueKind.MultiString:
                    return new string[] { };

                case RegistryValueKind.DWord:
                    return 0;

                case RegistryValueKind.QWord:
                    return (long)0;

                case RegistryValueKind.String:
                case RegistryValueKind.ExpandString:
                    return "";

                default:
                    return null;
            }
        }
    }
}