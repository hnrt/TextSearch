using Newtonsoft.Json;

namespace com.hideakin.textsearch.model
{
    internal class PathPositions
    {
        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("positions")]
        public int[] Positions { get; set; }
    }
}
