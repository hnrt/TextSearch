using System;
using System.Text;
using Newtonsoft.Json;

namespace com.hideakin.textsearch.model
{
    internal class UserInfo
    {
		[JsonProperty("uid")]
		public int Uid { get; set; }

		[JsonProperty("username")]
		public string Username { get; set; }

		[JsonProperty("roles")]
		public string[] Roles { get; set; }

		[JsonProperty("createdAt")]
		public DateTime CreatedAt { get; set; }

		[JsonProperty("updatedAt")]
		public DateTime UpdatedAt { get; set; }

		[JsonProperty("expiry")]
		public DateTime? Expiry { get; set; }

		[JsonProperty("apiKey")]
		public string ApiKey { get; set; }

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
	}
}
