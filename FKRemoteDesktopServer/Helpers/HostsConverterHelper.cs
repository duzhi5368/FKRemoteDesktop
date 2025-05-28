using FKRemoteDesktop.Structs;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Helpers
{
    public static class HostsConverterHelper
    {
        public static List<SHost> RawHostsToList(string rawHosts)
        {
            List<SHost> hostsList = new List<SHost>();
            if (string.IsNullOrEmpty(rawHosts)) 
                return hostsList;

            var hosts = rawHosts.Split(';');
            foreach (var host in hosts)
            {
                if ((string.IsNullOrEmpty(host) || !host.Contains(':'))) 
                    continue; // 无效HOST，忽略
                ushort port;
                if (!ushort.TryParse(host.Substring(host.LastIndexOf(':') + 1), out port)) 
                    continue; // 无效HOST，忽略
                hostsList.Add(new SHost { Hostname = host.Substring(0, host.LastIndexOf(':')), Port = port });
            }
            return hostsList;
        }

        public static string ListToRawHosts(IList<SHost> hosts)
        {
            StringBuilder rawHosts = new StringBuilder();
            foreach (var host in hosts)
                rawHosts.Append(host + ";");
            return rawHosts.ToString();
        }
    }
}
