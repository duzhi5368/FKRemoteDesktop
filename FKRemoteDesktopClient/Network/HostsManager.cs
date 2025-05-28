using FKRemoteDesktop.Structs;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Network
{
    public class HostsManager
    {
        private readonly Queue<SHost> _hosts = new Queue<SHost>();

        public HostsManager(List<SHost> hosts)
        {
            foreach (var host in hosts)
                _hosts.Enqueue(host);
        }

        public bool IsEmpty => _hosts.Count == 0;

        public SHost GetNextHost()
        {
            var temp = _hosts.Dequeue();
            _hosts.Enqueue(temp); // 添加到队列的尾部

            temp.IpAddress = ResolveHostname(temp);
            return temp;
        }

        private static IPAddress ResolveHostname(SHost host)
        {
            if (string.IsNullOrEmpty(host.Hostname)) return null;

            IPAddress ip;
            if (IPAddress.TryParse(host.Hostname, out ip))
            {
                if (ip.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    if (!Socket.OSSupportsIPv6) return null;
                }
                return ip;
            }

            var ipAddresses = Dns.GetHostEntry(host.Hostname).AddressList;
            foreach (IPAddress ipAddress in ipAddresses)
            {
                switch (ipAddress.AddressFamily)
                {
                    case AddressFamily.InterNetwork:
                        return ipAddress;

                    case AddressFamily.InterNetworkV6:
                        if (ipAddresses.Length == 1)
                            return ipAddress;
                        break;
                }
            }
            return ip;
        }
    }
}