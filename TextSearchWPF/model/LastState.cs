using Newtonsoft.Json;

namespace com.hideakin.textsearch.model
{
    internal class LastState
    {
        [JsonProperty("group")]
        public string Group { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("urls")]
        public string[] Urls { get; set; }
    }
}
