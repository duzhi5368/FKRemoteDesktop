//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message
{
    public interface ISender
    {
        void Send<T>(T message) where T : IMessage;
        void Disconnect();
    }
}
