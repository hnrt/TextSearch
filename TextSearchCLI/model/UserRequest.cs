using Newtonsoft.Json;

namespace com.hideakin.textsearch.model
{
    internal class UserRequest
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("roles")]
        public string[] Roles { get; set; }

        public UserRequest(string username, string password, string[] roles)
        {
            Username = username;
            Password = password;
            Roles = roles;
        }
    }
}
