using System;
using System.IO;
using Newtonsoft.Json;

namespace com.hideakin.textsearch.model
{
    internal class ApiCreadentialsCollection
    {
        [JsonProperty("last_user")]
        public string LastUser { get; set; }

        [JsonProperty("credentials")]
        public ApiCredentials[] Credentials;

        public ApiCreadentialsCollection()
        {
        }

        public static ApiCreadentialsCollection Load(string path)
        {
            if (File.Exists(path))
            {
                return JsonConvert.DeserializeObject<ApiCreadentialsCollection>(File.ReadAllText(path));
            }
            else
            {
                return null;
            }
        }

        public void Save(string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(this));
        }

        public ApiCredentials GetCredentials()
        {
            return GetCredentials(LastUser);
        }

        public ApiCredentials GetCredentials(string username)
        {
            foreach (var c in Credentials)
            {
                if (c.Username == username)
                {
                    return c;
                }
            }
            return null;
        }

        public void SetCredentials(ApiCredentials cred)
        {
            for (int index = 0; index < Credentials.Length; index++)
            {
                if (Credentials[index].Username == cred.Username)
                {
                    Credentials[index].EncryptedPassword = cred.EncryptedPassword;
                    Credentials[index].EncryptedToken = cred.EncryptedToken;
                    Credentials[index].ExpiresAt = cred.ExpiresAt;
                    return;
                }
            }
            Array.Resize(ref Credentials, Credentials.Length + 1);
            Credentials[Credentials.Length - 1] = new ApiCredentials(cred);
        }
    }
}
