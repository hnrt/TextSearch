using Newtonsoft.Json;

namespace com.hideakin.textsearch.model
{
    internal class UpdateIndexResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("textCount")]
        public int TextCount { get; set; }
    }
}
