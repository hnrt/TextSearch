using Newtonsoft.Json;

namespace com.hideakin.textsearch.model
{
    public class IdStatus
    {
        [JsonProperty("uid")]
        public int Uid { get; set; }

        [JsonProperty("gid")]
        public int Gid { get; set; }

        [JsonProperty("fid")]
        public int Fid { get; set; }

        public override string ToString()
        {
            return string.Format("UID.next={0} GID.next={1} FID.next={2}", Uid, Gid, Fid);
        }
    }
}
