﻿using FKRemoteDesktop.Helpers;
using FKRemoteDesktop.Structs;
using System.Globalization;
using System.IO;
using System.Net;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.IPGeoLocation
{
    public class GeoInformationRetriever
    {
        // 服务器端的国家图标列表
        private readonly string[] _imageList =
        {
            "ad", "ae", "af", "ag", "ai", "al",
            "am", "an", "ao", "ar", "as", "at", "au", "aw", "ax", "az", "ba",
            "bb", "bd", "be", "bf", "bg", "bh", "bi", "bj", "bm", "bn", "bo",
            "br", "bs", "bt", "bv", "bw", "by", "bz", "ca", "catalonia", "cc",
            "cd", "cf", "cg", "ch", "ci", "ck", "cl", "cm", "cn", "co", "cr",
            "cs", "cu", "cv", "cx", "cy", "cz", "de", "dj", "dk", "dm", "do",
            "dz", "ec", "ee", "eg", "eh", "england", "er", "es", "et",
            "europeanunion", "fam", "fi", "fj", "fk", "fm", "fo", "fr", "ga",
            "gb", "gd", "ge", "gf", "gh", "gi", "gl", "gm", "gn", "gp", "gq",
            "gr", "gs", "gt", "gu", "gw", "gy", "hk", "hm", "hn", "hr", "ht",
            "hu", "id", "ie", "il", "in", "io", "iq", "ir", "is", "it", "jm",
            "jo", "jp", "ke", "kg", "kh", "ki", "km", "kn", "kp", "kr", "kw",
            "ky", "kz", "la", "lb", "lc", "li", "lk", "lr", "ls", "lt", "lu",
            "lv", "ly", "ma", "mc", "md", "me", "mg", "mh", "mk", "ml", "mm",
            "mn", "mo", "mp", "mq", "mr", "ms", "mt", "mu", "mv", "mw", "mx",
            "my", "mz", "na", "nc", "ne", "nf", "ng", "ni", "nl", "no", "np",
            "nr", "nu", "nz", "om", "pa", "pe", "pf", "pg", "ph", "pk", "pl",
            "pm", "pn", "pr", "ps", "pt", "pw", "py", "qa", "re", "ro", "rs",
            "ru", "rw", "sa", "sb", "sc", "scotland", "sd", "se", "sg", "sh",
            "si", "sj", "sk", "sl", "sm", "sn", "so", "sr", "st", "sv", "sy",
            "sz", "tc", "td", "tf", "tg", "th", "tj", "tk", "tl", "tm", "tn",
            "to", "tr", "tt", "tv", "tw", "tz", "ua", "ug", "um", "us", "uy",
            "uz", "va", "vc", "ve", "vg", "vi", "vn", "vu", "wales", "wf",
            "ws", "ye", "yt", "za", "zm", "zw"
        };

        // 获取 IP 地理位置信息
        public SGeoInformation Retrieve()
        {
            var geo = TryRetrieveOnline() ?? TryRetrieveLocally();

            if (string.IsNullOrEmpty(geo.IpAddress))
                geo.IpAddress = TryGetWanIp();

            geo.IpAddress = (string.IsNullOrEmpty(geo.IpAddress)) ? "Unknown" : geo.IpAddress;
            geo.Country = (string.IsNullOrEmpty(geo.Country)) ? "Unknown" : geo.Country;
            geo.CountryCode = (string.IsNullOrEmpty(geo.CountryCode)) ? "-" : geo.CountryCode;
            geo.Timezone = (string.IsNullOrEmpty(geo.Timezone)) ? "Unknown" : geo.Timezone;
            geo.Asn = (string.IsNullOrEmpty(geo.Asn)) ? "Unknown" : geo.Asn;
            geo.Isp = (string.IsNullOrEmpty(geo.Isp)) ? "Unknown" : geo.Isp;

            geo.ImageIndex = 0;
            for (int i = 0; i < _imageList.Length; i++)
            {
                if (_imageList[i] == geo.CountryCode.ToLower())
                {
                    geo.ImageIndex = i;
                    break;
                }
            }
            if (geo.ImageIndex == 0)
                geo.ImageIndex = 247; // 问号
            return geo;
        }

        // 尝试网上获取本机地理位置信息
        private SGeoInformation TryRetrieveOnline()
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://ipwho.is/");
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:76.0) Gecko/20100101 Firefox/76.0";
                request.Proxy = null;
                request.Timeout = 10000;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (Stream dataStream = response.GetResponseStream())
                    {
                        var geoInfo = JsonHelper.Deserialize<SGeoResponse>(dataStream);
                        SGeoInformation g = new SGeoInformation
                        {
                            IpAddress = geoInfo.Ip,
                            Country = geoInfo.Country,
                            CountryCode = geoInfo.CountryCode,
                            Timezone = geoInfo.Timezone.UTC,
                            Asn = geoInfo.Connection.ASN.ToString(),
                            Isp = geoInfo.Connection.ISP
                        };
                        return g;
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        // 尝试本地检索地理位置信息
        private SGeoInformation TryRetrieveLocally()
        {
            try
            {
                SGeoInformation g = new SGeoInformation();

                var cultureInfo = CultureInfo.CurrentUICulture;
                var region = new RegionInfo(cultureInfo.LCID);
                g.Country = region.DisplayName;
                g.CountryCode = region.TwoLetterISORegionName;
                g.Timezone = DateTimeHelper.GetLocalTimeZone();

                return g;
            }
            catch
            {
                return null;
            }
        }

        // 尝试检索当前WAN IP信息
        private string TryGetWanIp()
        {
            string wanIp = "";
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.ipify.org/");
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:76.0) Gecko/20100101 Firefox/76.0";
                request.Proxy = null;
                request.Timeout = 5000;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (Stream dataStream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(dataStream))
                        {
                            wanIp = reader.ReadToEnd();
                        }
                    }
                }
            }
            catch { }
            return wanIp;
        }
    }
}