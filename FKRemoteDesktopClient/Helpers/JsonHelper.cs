using FKRemoteDesktop.Utilities;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

//--------------------------------------------------------------------------------------
namespace FKRemoteDesktop.Helpers
{
    // 序列化和反序列化Json格式
    public static class JsonHelper
    {
        public static string Serialize<T>(T o)
        {
            var s = new DataContractJsonSerializer(typeof(T));
            using (var ms = new MemoryStream())
            {
                s.WriteObject(ms, o);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        public static T Deserialize<T>(string json)
        {
            var s = new DataContractJsonSerializer(typeof(T));
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                return (T)s.ReadObject(ms);
            }
        }

        public static T Deserialize<T>(Stream stream)
        {
            var s = new DataContractJsonSerializer(typeof(T));
            return (T)s.ReadObject(stream);
        }

        public static JsonValue FromJSON(this string json)
        {
            return JsonValue.Load(new StringReader(json));
        }

        public static string ToJSON<T>(this T instance)
        {
            return JsonValue.ToJsonValue(instance);
        }
    }
}