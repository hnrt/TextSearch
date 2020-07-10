using Newtonsoft.Json;

namespace com.hideakin.textsearch.model
{
    internal class ValueResponse
    {
        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
