using FKRemoteDesktop.Enums;
using System;
using System.Security.Principal;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Structs
{
    public class SUserAccount
    {
        public string UserName { get; }
        public EAccountType Type { get; }

        public SUserAccount()
        {
            UserName = Environment.UserName;
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);

                if (principal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    Type = EAccountType.eAccountType_Admin;
                }
                else if (principal.IsInRole(WindowsBuiltInRole.User))
                {
                    Type = EAccountType.eAccountType_User;
                }
                else if (principal.IsInRole(WindowsBuiltInRole.Guest))
                {
                    Type = EAccountType.eAccountType_Guest;
                }
                else
                {
                    Type = EAccountType.eAccountType_Unknown;
                }
            }
        }
    }
}