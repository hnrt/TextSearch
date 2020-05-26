using Newtonsoft.Json;

namespace com.hideakin.textsearch.model
{
    internal class NameValuePair
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        public NameValuePair()
        {
        }

        public NameValuePair(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
