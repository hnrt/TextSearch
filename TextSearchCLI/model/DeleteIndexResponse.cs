using Newtonsoft.Json;

namespace com.hideakin.textsearch.model
{
    internal class DeleteIndexResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }
    }
}
