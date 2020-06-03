using Newtonsoft.Json;

namespace com.hideakin.textsearch.model
{
    internal class ValuesResponse
    {
        [JsonProperty("values")]
        public string[] Values { get; set; }
    }
}
