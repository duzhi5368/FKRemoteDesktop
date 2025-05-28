using System.Net.Security;
using System.Net;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Structs
{
    public class SPendingClient
    {
        public SslStream Stream { get; set; }
        public IPEndPoint EndPoint { get; set; }
    }
}
