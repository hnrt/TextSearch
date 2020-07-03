using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace com.hideakin.textsearch.model
{
    internal class FileStats
    {
        [JsonProperty("gid")]
		public int Gid { get; set; }

		[JsonProperty("group")]
		public string Group { get; set; }

		[JsonProperty("files")]
		public long Files { get; set; }

		[JsonProperty("totalBytes")]
		public long TotalBytes { get; set; }

		[JsonProperty("totalStoredBytes")]
		public long TotalStoredBytes { get; set; }

		[JsonProperty("staleFiles")]
		public long StaleFiles { get; set; }

		[JsonProperty("totalStaleBytes")]
		public long TotalStaleBytes { get; set; }

		[JsonProperty("totalStoredStaleBytes")]
		public long TotalStoredStaleBytes { get; set; }

        public override string ToString()
        {
			return string.Format("gid={0} group={1} files={2} bytes={3} stored={4} stale.files={5} stale.bytes={6} stale.stored={7}",
                Gid, Group, Files, TotalBytes, TotalStoredBytes, StaleFiles, TotalStaleBytes, TotalStoredStaleBytes);
        }
    }
}
