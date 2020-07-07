using System;
using Newtonsoft.Json;

namespace com.hideakin.textsearch.model
{
    internal class FileGroupInfo
    {
        private static readonly string DTFMT = "yyyy-MM-ddTHH:mm:ss.fff";

        [JsonProperty("gid")]
        public int Gid { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }

        public override string ToString()
        {
			return string.Format("[{0}] {1} created={2} updated={3}", Gid, Name, CreatedAt.ToString(DTFMT), UpdatedAt.ToString(DTFMT));
        }
    }
}
