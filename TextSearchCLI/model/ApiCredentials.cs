using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace com.hideakin.textsearch.model
{
    internal class ApiCredentials
    {
        private static readonly byte[] KEY;
        private static readonly byte[] IV;

        static ApiCredentials()
        {
            KEY = Encoding.UTF8.GetBytes(@"some considered to be unnatural."); // 256 bits : 32 bytes
            IV  = Encoding.UTF8.GetBytes(@"202006301439xxxx"); // BlockSize = 128 bits : 16 bytes
        }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string EncryptedPassword { get; set; }

        [JsonProperty("access_token")]
        public string EncryptedToken { get; set; }

        [JsonProperty("expires_at")]
        public DateTime ExpiresAt { get; set; }

        [JsonIgnore]
        public string Password
        {
            get
            {
                return EncryptedPassword != null ? Decrypt(EncryptedPassword) : null;
            }
            set
            {
                EncryptedPassword = value != null ? Encrypt(value) : null;
            }
        }

        [JsonIgnore]
        public string AccessToken
        {
            get
            {
                return Decrypt(EncryptedToken);
            }
            set
            {
                EncryptedToken = Encrypt(value);
            }
        }

        public ApiCredentials()
        {
        }

        public ApiCredentials(ApiCredentials src)
        {
            Username = src.Username;
            EncryptedPassword = src.EncryptedPassword;
            EncryptedToken = src.EncryptedToken;
            ExpiresAt = src.ExpiresAt;
        }

        private static string Encrypt(string value)
        {
            using (var rm = new RijndaelManaged() { KeySize = 256, BlockSize = 128 }) // AES-256
            using (var enc = rm.CreateEncryptor(KEY, IV))
            using (var dst = new MemoryStream())
            using (var cs = new CryptoStream(dst, enc, CryptoStreamMode.Write))
            {
                var data = Encoding.UTF8.GetBytes(value);
                cs.Write(data, 0, data.Length);
                cs.FlushFinalBlock();
                return Convert.ToBase64String(dst.ToArray());
            }
        }

        private static string Decrypt(string value)
        {
            using (var rm = new RijndaelManaged() { KeySize = 256, BlockSize = 128 }) // AES-256
            using (var dec = rm.CreateDecryptor(KEY, IV))
            using (var src = new MemoryStream(Convert.FromBase64String(value), false))
            using (var dst = new MemoryStream())
            using (var cs = new CryptoStream(src, dec, CryptoStreamMode.Read))
            {
                var buf = new byte[4096];
                while (true)
                {
                    int len = cs.Read(buf, 0, buf.Length);
                    if (len <= 0)
                    {
                        break;
                    }
                    dst.Write(buf, 0, len);
                }
                return Encoding.UTF8.GetString(dst.ToArray());
            }
        }
    }
}
