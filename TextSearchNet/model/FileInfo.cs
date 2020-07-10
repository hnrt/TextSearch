using Newtonsoft.Json;

namespace com.hideakin.textsearch.model
{
    public class FileInfo
    {
        [JsonProperty("fid")]
        public int Fid { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("size")]
        public int Size { get; set; }

        [JsonProperty("gid")]
        public int Gid { get; set; }

        [JsonProperty("group")]
        public string Group { get; set; }
    }
}
