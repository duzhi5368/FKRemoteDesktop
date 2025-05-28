using System.Runtime.Serialization;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Structs
{
    [DataContract]
    public class SGeoResponse
    {
        [DataMember(Name = "ip")]
        public string Ip { get; set; }

        [DataMember(Name = "continent_code")]
        public string ContinentCode { get; set; }

        [DataMember(Name = "country")]
        public string Country { get; set; }

        [DataMember(Name = "country_code")]
        public string CountryCode { get; set; }

        [DataMember(Name = "timezone")]
        public STime Timezone { get; set; }

        [DataMember(Name = "connection")]
        public SConn Connection { get; set; }
    }

    [DataContract]
    public class STime
    {
        [DataMember(Name = "utc")]
        public string UTC { get; set; }
    }

    [DataContract]
    public class SConn
    {
        [DataMember(Name = "asn")]
        public string ASN { get; set; }

        [DataMember(Name = "isp")]
        public string ISP { get; set; }
    }
}