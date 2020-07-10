using Newtonsoft.Json;

namespace com.hideakin.textsearch.model
{
    internal class FileGroupRequest
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        public FileGroupRequest(string name)
        {
            Name = name;
        }
    }
}
