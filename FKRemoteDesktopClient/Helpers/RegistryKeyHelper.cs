using FKRemoteDesktop.Extensions;
using FKRemoteDesktop.Message.MessageStructs;
using FKRemoteDesktop.Utilities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Helpers
{
    public static class RegistryKeyHelper
    {
        private static string DEFAULT_VALUE = String.Empty;

        public static bool AddRegistryKeyValue(RegistryHive hive, string path, string name, string value, bool addQuotes = false)
        {
            try
            {
                using (RegistryKey key = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64).OpenWritableSubKeySafe(path))
                {
                    if (key == null)
                        return false;
                    if (addQuotes && !value.StartsWith("\"") && !value.EndsWith("\""))
                        value = "\"" + value + "\"";
                    key.SetValue(name, value);
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static RegistryKey OpenReadonlySubKey(RegistryHive hive, string path)
        {
            try
            {
                return RegistryKey.OpenBaseKey(hive, RegistryView.Registry64).OpenSubKey(path, false);
            }
            catch
            {
                return null;
            }
        }

        public static bool DeleteRegistryKeyValue(RegistryHive hive, string path, string name)
        {
            try
            {
                using (RegistryKey key = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64).OpenWritableSubKeySafe(path))
                {
                    if (key == null)
                        return false;
                    key.DeleteValue(name, true);
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsDefaultValue(string valueName)
        {
            return String.IsNullOrEmpty(valueName);
        }

        public static RegValueData[] AddDefaultValue(List<RegValueData> values)
        {
            if (!values.Any(value => IsDefaultValue(value.Name)))
            {
                values.Add(GetDefaultValue());
            }
            return values.ToArray();
        }

        public static RegValueData[] GetDefaultValues()
        {
            return new[] { GetDefaultValue() };
        }

        public static RegValueData CreateRegValueData(string name, RegistryValueKind kind, object value = null)
        {
            var newRegValue = new RegValueData { Name = name, Kind = kind };
            if (value == null)
                newRegValue.Data = new byte[] { };
            else
            {
                switch (newRegValue.Kind)
                {
                    case RegistryValueKind.Binary:
                        newRegValue.Data = (byte[])value;
                        break;

                    case RegistryValueKind.MultiString:
                        newRegValue.Data = ByteConverter.GetBytes((string[])value);
                        break;

                    case RegistryValueKind.DWord:
                        newRegValue.Data = ByteConverter.GetBytes((uint)(int)value);
                        break;

                    case RegistryValueKind.QWord:
                        newRegValue.Data = ByteConverter.GetBytes((ulong)(long)value);
                        break;

                    case RegistryValueKind.String:
                    case RegistryValueKind.ExpandString:
                        newRegValue.Data = ByteConverter.GetBytes((string)value);
                        break;
                }
            }
            return newRegValue;
        }

        private static RegValueData GetDefaultValue()
        {
            return CreateRegValueData(DEFAULT_VALUE, RegistryValueKind.String);
        }
    }
}