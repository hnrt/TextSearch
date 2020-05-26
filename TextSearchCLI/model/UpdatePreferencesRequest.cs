using Newtonsoft.Json;

namespace com.hideakin.textsearch.model
{
    internal class UpdatePreferencesRequest
    {
        [JsonProperty("prefs")]
        public NameValuePair[] Prefs { get; set; }
    }
}
