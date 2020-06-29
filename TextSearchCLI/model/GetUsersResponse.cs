using Newtonsoft.Json;

namespace com.hideakin.textsearch.model
{
    internal class GetUsersResponse
    {
        [JsonProperty("values")]
        public UserInfo[] Values { get; set; }
    }
}
