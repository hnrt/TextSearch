using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace com.hideakin.textsearch.model
{
    internal class FileInfo
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
