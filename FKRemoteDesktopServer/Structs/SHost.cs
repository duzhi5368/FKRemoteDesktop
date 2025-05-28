using System.Net;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Structs
{
    public class SHost
    {
        public string Hostname { get; set; }            // 可以是 IPv4, IPv6, 或主机名
        public IPAddress IpAddress { get; set; }        // 可以是 IPv4, IPv6
        public ushort Port { get; set; }

        public override string ToString()
        {
            return Hostname + ":" + Port;
        }
    }
}
