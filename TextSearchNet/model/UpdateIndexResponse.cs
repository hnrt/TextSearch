using Newtonsoft.Json;

namespace com.hideakin.textsearch.model
{
    internal class UpdateIndexResponse
    {
        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("texts")]
        public string[] Texts { get; set; }
    }
}
