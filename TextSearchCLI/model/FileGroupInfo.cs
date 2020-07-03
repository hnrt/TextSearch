using System.Text;
using Newtonsoft.Json;

namespace com.hideakin.textsearch.model
{
    internal class FileGroupInfo
    {
        [JsonProperty("gid")]
        public int Gid { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("ownedBy")]
        public string[] OwnedBy { get; set; }

		public string OwnedByString
		{
			get
			{
				var sb = new StringBuilder();
				if (OwnedBy.Length > 0)
				{
					sb.Append(OwnedBy[0]);
					for (int index = 1; index < OwnedBy.Length; index++)
					{
						sb.Append(",");
						sb.Append(OwnedBy[index]);
					}
				}
				return sb.ToString();
			}
		}

        public override string ToString()
        {
			return string.Format("gid={0} name={1} ownedBy={2}", Gid, Name, OwnedByString);
        }
    }
}
