using Newtonsoft.Json;

namespace com.hideakin.textsearch.model
{
    internal class LastState
    {
        [JsonProperty("group")]
        public string Group { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }
    }
}
