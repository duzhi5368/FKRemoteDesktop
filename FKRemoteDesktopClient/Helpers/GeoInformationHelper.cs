using FKRemoteDesktop.IPGeoLocation;
using FKRemoteDesktop.Structs;
using System;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Helpers
{
    // 检查当前客户端最后的 IP 地址位置信息
    public static class GeoInformationHelper
    {
        private const int MINIMUM_VALID_TIME = 60 * 15; // 每此检索IP位置的间隔时间

        // 用于获取 WAN IP 信息
        private static readonly GeoInformationRetriever Retriever = new GeoInformationRetriever();

        // 存储当前 IP 信息
        private static SGeoInformation _geoInformation;

        // 上次成功检索 IP 的时间
        private static DateTime _lastSuccessfulLocation = new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static SGeoInformation GetGeoInformation()
        {
            var passedTime = new TimeSpan(DateTime.UtcNow.Ticks - _lastSuccessfulLocation.Ticks);
            if (_geoInformation == null || passedTime.TotalMinutes > MINIMUM_VALID_TIME)
            {
                _geoInformation = Retriever.Retrieve();
                _lastSuccessfulLocation = DateTime.UtcNow;
            }
            return _geoInformation;
        }
    }
}