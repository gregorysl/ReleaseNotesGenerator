using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RNA.Model
{
    public class ServerDetails
    {
        public string Url { get; set; }
        public string Pat { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public ServerTypeEnum ServerType { get; set; }
    }
}
