using Newtonsoft.Json;

namespace SimpleTCPPlus.Common.JSON
{
    public static class JsonUtils
    {
        public static T DeserializeIt<T>(string data) => (T)JsonConvert.DeserializeObject<T>(data);
        public static string SerializeIt(object obj) => JsonConvert.SerializeObject(obj);
    }
}
