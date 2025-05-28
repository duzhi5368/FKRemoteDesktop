using Open.Nat;
using System;
using System.Collections.Generic;
using System.Threading;
//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Network
{
    public class UPnPService
    {
        // 用于跟踪所有创建的映射
        private readonly Dictionary<int, Mapping> _mappings = new Dictionary<int, Mapping>();
        private NatDevice _device;                  // 已发现的UPnP设备
        private NatDiscoverer _discoverer;          // 用于NAT设备的发现器

        // 初始化 UPnP 发现器
        public UPnPService()
        {
            _discoverer = new NatDiscoverer();
        }

        /// <summary>
        /// 在 UPnP 设备上创建一个新的端口映射
        /// </summary>
        /// <param name="port">进行映射的端口</param>
        public async void CreatePortMapAsync(int port)
        {
            try
            {
                var cts = new CancellationTokenSource(10000);
                _device = await _discoverer.DiscoverDeviceAsync(PortMapper.Upnp, cts);

                Mapping mapping = new Mapping(Protocol.Tcp, port, port);
                await _device.CreatePortMapAsync(mapping);

                if (_mappings.ContainsKey(mapping.PrivatePort))
                    _mappings[mapping.PrivatePort] = mapping;
                else
                    _mappings.Add(mapping.PrivatePort, mapping);
            }
            catch (Exception ex) when (ex is MappingException || ex is NatDeviceNotFoundException)
            {
            }
        }

        /// <summary>
        /// 删除现有的端口映射
        /// </summary>
        /// <param name="port">需要删除的端口</param>
        public async void DeletePortMapAsync(int port)
        {
            if (_mappings.TryGetValue(port, out var mapping))
            {
                try
                {
                    await _device.DeletePortMapAsync(mapping);
                    _mappings.Remove(mapping.PrivatePort);
                }
                catch (Exception ex) when (ex is MappingException || ex is NatDeviceNotFoundException)
                {
                }
            }
        }
    }
}
