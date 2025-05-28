using FKRemoteDesktop.Structs;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Install
{
    public abstract class ClientSetupBase
    {
        protected SUserAccount UserAccount;

        protected ClientSetupBase()
        {
            UserAccount = new SUserAccount();
        }
    }
}