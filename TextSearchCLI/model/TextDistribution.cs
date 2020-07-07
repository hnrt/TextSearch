using Newtonsoft.Json;

namespace com.hideakin.textsearch.model
{
    internal class TextDistribution
    {
        [JsonProperty("fid")]
        public int Fid { get; set; }

        [JsonProperty("positions")]
        public int[] Positions { get; set; }
    }
}
