using System;
using System.Text;
using Newtonsoft.Json;
using com.hideakin.textsearch.utility;

namespace com.hideakin.textsearch.model
{
    internal class UserInfo
    {
		private static readonly string DTFMT = "yyyy-MM-ddTHH:mm:ss.fff";

		[JsonProperty("uid")]
		public int Uid { get; set; }

		[JsonProperty("username")]
		public string Username { get; set; }

		[JsonProperty("roles")]
		public string[] Roles { get; set; }

		[JsonProperty("created_at")]
		public DateTime CreatedAt { get; set; }

		[JsonProperty("updated_at")]
		public DateTime UpdatedAt { get; set; }

		[JsonProperty("access_token")]
		public string AccessToken { get; set; }

		[JsonProperty("expires_at")]
		public DateTime? ExpiresAt { get; set; }

		public string RolesString
		{
			get
			{
				var sb = new StringBuilder();
				if (Roles.Length > 0)
				{
					sb.Append(Roles[0]);
					for (int index = 1; index < Roles.Length; index++)
					{
						sb.Append(",");
						sb.Append(Roles[index]);
					}
				}
				return sb.ToString();
			}
		}

        public override string ToString()
        {
			if (ExpiresAt != null)
			{
				return string.Format("uid={0} username={1} roles={2} created={3} updated={4} token={5} expires={6}",
					Uid, Username, RolesString, CreatedAt.ToString(DTFMT), UpdatedAt.ToString(DTFMT),
					AccessToken, ExpiresAt.Value.ToString(DTFMT));
			}
			else
			{
				return string.Format("uid={0} username={1} roles={2} created={3} updated={4} token= expires=",
					Uid, Username, RolesString, CreatedAt.ToString(DTFMT), UpdatedAt.ToString(DTFMT));
			}
        }
    }
}
