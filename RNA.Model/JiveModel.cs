using Newtonsoft.Json;

namespace RNA.Model
{
    public class SearchResponse
    {
        [JsonProperty("list")]
        public Result[] List { get; set; }
    }

    public class Result
    {
        [JsonProperty("subject")]
        public string Subject { get; set; }
        [JsonProperty("contentID")]
        public string ContentId { get; set; }
        [JsonProperty("resources")]
        public Resources Resources { get; set; }
    }

    public class Resources
    {
        [JsonProperty("html")]
        public Html Html { get; set; }
    }

    public class Html
    {
        [JsonProperty("ref")]
        public string Link { get; set; }

        public override string ToString()
        {
            return Link;
        }
    }
}
