using FKRemoteDesktop.Debugger;
using FKRemoteDesktop.Enums;
using FKRemoteDesktop.Helpers;
using FKRemoteDesktop.Message.SubMessages;
using FKRemoteDesktop.Structs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Message.SubMessageHandler
{
    // 处理与远程系统信息交互的消息
    public class SystemInformationHandler : MessageProcessorBase<List<Tuple<string, string>>>
    {
        public SystemInformationHandler() : base(true)
        {
        }

        public override bool CanExecute(IMessage message) => message is GetSystemInfo;

        public override bool CanExecuteFrom(ISender client) => true;

        public override void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case GetSystemInfo info:
                    Logger.Log(ELogType.eLogType_Debug, "收到服务器消息: GetSystemInfo");
                    Execute(sender, info);
                    break;
            }
        }

        private void Execute(ISender client, GetSystemInfo message)
        {
            try
            {
                IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();

                var domainName = (!string.IsNullOrEmpty(properties.DomainName)) ? properties.DomainName : "-";
                var hostName = (!string.IsNullOrEmpty(properties.HostName)) ? properties.HostName : "-";

                var geoInfo = GeoInformationHelper.GetGeoInformation();
                var userAccount = new SUserAccount();

                List<Tuple<string, string>> lstInfos = new List<Tuple<string, string>>
                {
                    new Tuple<string, string>("CPU 名", HardwareDevicesHelper.CpuName),
                    new Tuple<string, string>("内存", $"{HardwareDevicesHelper.TotalPhysicalMemory} MB"),
                    new Tuple<string, string>("GPU 名", HardwareDevicesHelper.GpuName),
                    new Tuple<string, string>("用户名", userAccount.UserName),
                    new Tuple<string, string>("PC 名", SystemHelper.GetPcName()),
                    new Tuple<string, string>("域名", domainName),
                    new Tuple<string, string>("主机名", hostName),
                    new Tuple<string, string>("系统驱动器", Path.GetPathRoot(Environment.SystemDirectory)),
                    new Tuple<string, string>("系统目录", Environment.SystemDirectory),
                    new Tuple<string, string>("系统已启动时间", SystemHelper.GetUptime()),
                    new Tuple<string, string>("MAC 地址", HardwareDevicesHelper.MacAddress),
                    new Tuple<string, string>("LAN IP 地址", HardwareDevicesHelper.LanIpAddress),
                    new Tuple<string, string>("WAN IP 地址", geoInfo.IpAddress),
                    new Tuple<string, string>("ASN 自治系统编号", geoInfo.Asn),
                    new Tuple<string, string>("ISP 运行商", geoInfo.Isp),
                    new Tuple<string, string>("病毒软件列表", SystemHelper.GetAntivirus()),
                    new Tuple<string, string>("防火墙列表", SystemHelper.GetFirewall()),
                    new Tuple<string, string>("所属时区", geoInfo.Timezone),
                    new Tuple<string, string>("所属国家", geoInfo.Country)
                };

                client.Send(new GetSystemInfoResponse { SystemInfos = lstInfos });
            }
            catch
            {
            }
        }
    }
}