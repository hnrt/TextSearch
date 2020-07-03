using Newtonsoft.Json;

namespace com.hideakin.textsearch.model
{
    internal class FileGroupRequest
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("ownedBy")]
        public string[] OwnedBy { get; set; }

        public FileGroupRequest(string name, string[] ownedBy)
        {
            Name = name;
            OwnedBy = ownedBy;
        }
    }
}
