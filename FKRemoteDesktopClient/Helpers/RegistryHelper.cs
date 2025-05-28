using FKRemoteDesktop.Extensions;
using FKRemoteDesktop.Message.MessageStructs;
using FKRemoteDesktop.Structs;
using Microsoft.Win32;
using System;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Helpers
{
    public static class RegistryHelper
    {
        private const string REGISTRY_KEY_CREATE_ERROR = "无法创建注册表项：写入注册表时出错。";
        private const string REGISTRY_KEY_DELETE_ERROR = "无法删除注册表项：写入注册表时出错。";
        private const string REGISTRY_KEY_RENAME_ERROR = "无法重命名注册表项：写入注册表时出错。";
        private const string REGISTRY_VALUE_CREATE_ERROR = "无法创建注册表值：写入注册表时出错。";
        private const string REGISTRY_VALUE_DELETE_ERROR = "无法删除注册表值：写入注册表时出错。";
        private const string REGISTRY_VALUE_RENAME_ERROR = "无法重命名注册表值：写入注册表时出错。";
        private const string REGISTRY_VALUE_CHANGE_ERROR = "无法修改注册表值：写入注册表时出错。";

        public static RegistryKey GetWritableRegistryKey(string keyPath)
        {
            RegistryKey key = SRegistrySeeker.GetRootKey(keyPath);
            if (key != null)
            {
                if (key.Name != keyPath)
                {
                    string subKeyName = keyPath.Substring(key.Name.Length + 1);
                    key = key.OpenWritableSubKeySafe(subKeyName);
                }
            }
            return key;
        }

        public static bool CreateRegistryKey(string parentPath, out string name, out string errorMsg)
        {
            name = "";
            try
            {
                RegistryKey parent = GetWritableRegistryKey(parentPath);
                if (parent == null)
                {
                    errorMsg = "当前没有注册表的写权限: " + parentPath + "，请使用权限提升，以管理员身份运行客户端";
                    return false;
                }

                int i = 1;
                string testName = String.Format("New Key #{0}", i);
                while (parent.ContainsSubKey(testName))
                {
                    i++;
                    testName = String.Format("New Key #{0}", i);
                }
                name = testName;
                using (RegistryKey child = parent.CreateSubKeySafe(name))
                {
                    if (child == null)
                    {
                        errorMsg = REGISTRY_KEY_CREATE_ERROR;
                        return false;
                    }
                }
                errorMsg = "";
                return true;
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                return false;
            }
        }

        public static bool DeleteRegistryKey(string name, string parentPath, out string errorMsg)
        {
            try
            {
                RegistryKey parent = GetWritableRegistryKey(parentPath);
                if (parent == null)
                {
                    errorMsg = "当前没有注册表的写权限: " + parentPath + "，请使用权限提升，以管理员身份运行客户端";
                    return false;
                }
                if (!parent.ContainsSubKey(name))
                {
                    errorMsg = "路径 " + parentPath + " 中未存在注册表项 " + name;
                    return true;
                }

                bool success = parent.DeleteSubKeyTreeSafe(name);
                if (!success)
                {
                    errorMsg = REGISTRY_KEY_DELETE_ERROR;
                    return false;
                }
                errorMsg = "";
                return true;
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                return false;
            }
        }

        public static bool RenameRegistryKey(string oldName, string newName, string parentPath, out string errorMsg)
        {
            try
            {
                RegistryKey parent = GetWritableRegistryKey(parentPath);
                if (parent == null)
                {
                    errorMsg = "当前没有注册表的写权限: " + parentPath + "，请使用权限提升，以管理员身份运行客户端";
                    return false;
                }
                if (!parent.ContainsSubKey(oldName))
                {
                    errorMsg = "路径 " + parentPath + " 中未存在注册表项 " + oldName;
                    return false;
                }

                bool success = parent.RenameSubKeySafe(oldName, newName);
                if (!success)
                {
                    errorMsg = REGISTRY_KEY_RENAME_ERROR;
                    return false;
                }
                errorMsg = "";
                return true;
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                return false;
            }
        }

        public static bool CreateRegistryValue(string keyPath, RegistryValueKind kind, out string name, out string errorMsg)
        {
            name = "";
            try
            {
                RegistryKey key = GetWritableRegistryKey(keyPath);
                if (key == null)
                {
                    errorMsg = "当前没有注册表的写权限: " + keyPath + "，请使用权限提升，以管理员身份运行客户端";
                    return false;
                }

                int i = 1;
                string testName = String.Format("New Value #{0}", i);

                while (key.ContainsValue(testName))
                {
                    i++;
                    testName = String.Format("New Value #{0}", i);
                }
                name = testName;

                bool success = key.SetValueSafe(name, kind.GetDefault(), kind);
                if (!success)
                {
                    errorMsg = REGISTRY_VALUE_CREATE_ERROR;
                    return false;
                }
                errorMsg = "";
                return true;
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                return false;
            }
        }

        public static bool DeleteRegistryValue(string keyPath, string name, out string errorMsg)
        {
            try
            {
                RegistryKey key = GetWritableRegistryKey(keyPath);
                if (key == null)
                {
                    errorMsg = "当前没有注册表的写权限: " + keyPath + "，请使用权限提升，以管理员身份运行客户端";
                    return false;
                }
                if (!key.ContainsValue(name))
                {
                    errorMsg = "路径 " + keyPath + " 中未存在注册表项 " + name;
                    return true;
                }

                bool success = key.DeleteValueSafe(name);
                if (!success)
                {
                    errorMsg = REGISTRY_VALUE_DELETE_ERROR;
                    return false;
                }
                errorMsg = "";
                return true;
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                return false;
            }
        }

        public static bool RenameRegistryValue(string oldName, string newName, string keyPath, out string errorMsg)
        {
            try
            {
                RegistryKey key = GetWritableRegistryKey(keyPath);
                if (key == null)
                {
                    errorMsg = "当前没有注册表的写权限: " + keyPath + "，请使用权限提升，以管理员身份运行客户端";
                    return false;
                }
                if (!key.ContainsValue(oldName))
                {
                    errorMsg = "路径 " + keyPath + " 中未存在注册表项 " + oldName;
                    return false;
                }

                bool success = key.RenameValueSafe(oldName, newName);
                if (!success)
                {
                    errorMsg = REGISTRY_VALUE_RENAME_ERROR;
                    return false;
                }
                errorMsg = "";
                return true;
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                return false;
            }
        }

        public static bool ChangeRegistryValue(RegValueData value, string keyPath, out string errorMsg)
        {
            try
            {
                RegistryKey key = GetWritableRegistryKey(keyPath);
                if (key == null)
                {
                    errorMsg = "当前没有注册表的写权限: " + keyPath + "，请使用权限提升，以管理员身份运行客户端";
                    return false;
                }
                if (!RegistryKeyHelper.IsDefaultValue(value.Name) && !key.ContainsValue(value.Name))
                {
                    errorMsg = "路径 " + keyPath + " 中未存在注册表项 " + value.Name;
                    return false;
                }

                bool success = key.SetValueSafe(value.Name, value.Data, value.Kind);
                if (!success)
                {
                    errorMsg = REGISTRY_VALUE_CHANGE_ERROR;
                    return false;
                }
                errorMsg = "";
                return true;
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                return false;
            }
        }
    }
}