using System;
using Newtonsoft.Json;

namespace com.hideakin.textsearch.model
{
    public class IndexStats
    {
        [JsonProperty("gid")]
        public int Gid { get; set; }

        [JsonProperty("group")]
        public String Group { get; set; }

        [JsonProperty("file_count")]
        public int FileCount { get; set; }

        [JsonProperty("text_count")]
        public int TextCount { get; set; }

        [JsonProperty("total_bytes")]
        public long TotalBytes { get; set; }

        [JsonProperty("total_count")]
        public long TotalCount { get; set; }

        public override string ToString()
        {
            if (TotalCount == 0)
            {
                return string.Format("gid={0} group={1} files={2:#,0} texts={3:#,0} bytes={4:#,0} data={5:#,0}",
                    Gid, Group, FileCount, TextCount, TotalBytes, TotalCount);
            }
            return string.Format("gid={0} group={1} files={2:#,0} texts={3:#,0} bytes={4:#,0} data={5:#,0} ({6}%)",
                Gid, Group, FileCount, TextCount, TotalBytes, TotalCount, 100 * TotalBytes / (4 * TotalCount));
        }
    }
}
