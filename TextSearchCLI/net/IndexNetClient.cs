using com.hideakin.textsearch.data;
using com.hideakin.textsearch.model;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace com.hideakin.textsearch.net
{
    internal class IndexNetClient
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public static string Url { get; set; } = @"http://localhost:8080";

        public static ApiCredentials Credentials { get; } = new ApiCredentials();

        static IndexNetClient()
        {
            var envUrl = Environment.GetEnvironmentVariable("TEXTINDEXAPI_URL");
            if (envUrl != null)
            {
                Url = envUrl;
            }
            var envUsername = Environment.GetEnvironmentVariable("TEXTINDEXAPI_USERNAME");
            if (envUsername != null)
            {
                Credentials.Username = envUsername;
            }
            var envPassword = Environment.GetEnvironmentVariable("TEXTINDEXAPI_PASSWORD");
            if (envPassword != null)
            {
                Credentials.Password = envPassword;
            }
        }

        public string GroupName { get; set; } = "default";

        private CancellationTokenSource cts = new CancellationTokenSource();

        public HttpResponseMessage Response { get; private set; }

        public string ResponseBody { get; private set; }

        public IndexNetClient()
        {
            if (Credentials.EncryptedToken == null)
            {
                Initialize();
            }
        }

        #region SETUP

        private void Initialize()
        {
            ApiCreadentialsCollection cc;
            var path = GetFilePath();
            if (File.Exists(path))
            {
                cc = ApiCreadentialsCollection.Load(path);
                if (Credentials.EncryptedPassword == null)
                {
                    if (Credentials.Username != null)
                    {
                        var c = cc.GetCredentials(Credentials.Username);
                        if (c != null)
                        {
                            Credentials.EncryptedPassword = c.EncryptedPassword;
                            Credentials.EncryptedToken = c.EncryptedToken;
                            Credentials.ExpiresAt = c.ExpiresAt;
                        }
                        cc.LastUser = Credentials.Username;
                    }
                    else
                    {
                        var c = cc.GetCredentials();
                        if (c != null)
                        {
                            Credentials.Username = c.Username;
                            Credentials.EncryptedPassword = c.EncryptedPassword;
                            Credentials.EncryptedToken = c.EncryptedToken;
                            Credentials.ExpiresAt = c.ExpiresAt;
                        }
                    }
                }
            }
            else
            {
                cc = new ApiCreadentialsCollection()
                {
                    LastUser = null,
                    Credentials = new ApiCredentials[0]
                };
            }
            if (Credentials.EncryptedToken == null || Credentials.ExpiresAt <= DateTime.Now.AddMinutes(1))
            {
                if (Credentials.Username == null || Credentials.EncryptedPassword == null)
                {
                    throw new Exception("No valid API key is available. Credentials need to be specified.");
                }
                var task = Authenticate();
                task.Wait();
                if (task.Result is AuthenticateErrorResponse)
                {
                    throw new Exception(((AuthenticateErrorResponse)task.Result).ErrorDescription);
                }
                var ar = (AuthenticateResponse)task.Result;
                Credentials.AccessToken = ar.AccessToken;
                Credentials.ExpiresAt = DateTime.Now.AddSeconds(ar.ExpiresIn);
            }
            cc.SetCredentials(Credentials);
            cc.LastUser = Credentials.Username;
            cc.Save(path);
        }

        private string GetFilePath()
        {
            return Path.Combine(GetDirPath(), string.Format("credentials.json"));
        }

        private string GetDirPath()
        {
            var dir1 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HNRT");
            if (!Directory.Exists(dir1))
            {
                Directory.CreateDirectory(dir1);
            }
            var dir2 = Path.Combine(dir1, "TextSearch");
            if (!Directory.Exists(dir2))
            {
                Directory.CreateDirectory(dir2);
            }
            return dir2;
        }

        #endregion

        #region AUTHENTICATION

        private async Task<object> Authenticate()
        {
            var uri = string.Format("{0}/v1/authentication", Url);
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add("Authorization", BasicToken);
            Response = await httpClient.SendAsync(request, cts.Token);
            ResponseBody = await Response.Content.ReadAsStringAsync();
            if (Response.StatusCode == HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<AuthenticateResponse>(ResponseBody);
            }
            else
            {
                return JsonConvert.DeserializeObject<AuthenticateErrorResponse>(ResponseBody);
            }
        }

        private string BasicToken
        {
            get
            {
                var s = string.Format("{0}:{1}", Credentials.Username, Credentials.Password);
                return "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(s));
            }
        }

        private string BearerToken
        {
            get
            {
                return "Bearer " + Credentials.AccessToken;
            }
        }

        #endregion

        #region MAINTENANCE

        public async Task<bool> Check()
        {
            var uri = string.Format("{0}/v1/maintenance", Url);
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            Response = await httpClient.SendAsync(request, cts.Token);
            ResponseBody = await Response.Content.ReadAsStringAsync();
            return Response.StatusCode == HttpStatusCode.OK && ResponseBody == "false";
        }

        #endregion

        #region USER

        public async Task<UserInfo[]> GetUsers()
        {
            var uri = string.Format("{0}/v1/users", Url);
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add("Authorization", BearerToken);
            Response = await httpClient.SendAsync(request, cts.Token);
            ResponseBody = await Response.Content.ReadAsStringAsync();
            if (Response.StatusCode == HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<GetUsersResponse>(ResponseBody).Values;
            }
            else
            {
                return null;
            }
        }

        public async Task<UserInfo> GetUser(string username)
        {
            var uri = string.Format("{0}/v1/users/{1}", Url, username);
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add("Authorization", BearerToken);
            Response = await httpClient.SendAsync(request, cts.Token);
            ResponseBody = await Response.Content.ReadAsStringAsync();
            if (Response.StatusCode == HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<UserInfo>(ResponseBody);
            }
            else
            {
                return null;
            }
        }

        public async Task<UserInfo> CreateUser(string username, string password, string[] roles)
        {
            var uri = string.Format("{0}/v1/users", Url);
            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Add("Authorization", BearerToken);
            var input = new UserRequest(username, password, roles);
            request.Content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8, "application/json");
            Response = await httpClient.SendAsync(request, cts.Token);
            ResponseBody = await Response.Content.ReadAsStringAsync();
            if (Response.StatusCode == HttpStatusCode.Created)
            {
                return JsonConvert.DeserializeObject<UserInfo>(ResponseBody);
            }
            else
            {
                return null;
            }
        }

        public async Task<UserInfo> UpdateUser(string username, string password, string[] roles)
        {
            var uri = string.Format("{0}/v1/users", Url);
            var request = new HttpRequestMessage(HttpMethod.Put, uri);
            request.Headers.Add("Authorization", BearerToken);
            var input = new UserRequest(username, password, roles);
            request.Content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8, "application/json");
            Response = await httpClient.SendAsync(request, cts.Token);
            ResponseBody = await Response.Content.ReadAsStringAsync();
            if (Response.StatusCode == HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<UserInfo>(ResponseBody);
            }
            else
            {
                return null;
            }
        }

        public async Task<UserInfo> DeleteUser(string username)
        {
            var uri = string.Format("{0}/v1/users/{1}", Url, username);
            var request = new HttpRequestMessage(HttpMethod.Delete, uri);
            request.Headers.Add("Authorization", BearerToken);
            Response = await httpClient.SendAsync(request, cts.Token);
            ResponseBody = await Response.Content.ReadAsStringAsync();
            if (Response.StatusCode == HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<UserInfo>(ResponseBody);
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region PREFERENCE

        public async Task<string> GetPreference(string name)
        {
            var uri = string.Format("{0}/v1/preferences/{1}", Url, name);
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add("Authorization", BearerToken);
            Response = await httpClient.SendAsync(request, cts.Token);
            ResponseBody = await Response.Content.ReadAsStringAsync();
            if (Response.StatusCode == HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<ValueResponse>(ResponseBody).Value;
            }
            else
            {
                return null;
            }
        }

        public async Task<bool> UpdatePreference(string name, string value)
        {
            var uri = string.Format("{0}/v1/preferences", Url);
            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Add("Authorization", BearerToken);
            var input = new UpdatePreferenceRequest(name, value);
            request.Content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8, "application/json");
            Response = await httpClient.SendAsync(request, cts.Token);
            ResponseBody = await Response.Content.ReadAsStringAsync();
            return Response.StatusCode == HttpStatusCode.OK;
        }

        public async Task<bool> DeletePreference(string name)
        {
            var uri = string.Format("{0}/v1/preferences/{1}", Url, name);
            var request = new HttpRequestMessage(HttpMethod.Delete, uri);
            request.Headers.Add("Authorization", BearerToken);
            Response = await httpClient.SendAsync(request, cts.Token);
            ResponseBody = await Response.Content.ReadAsStringAsync();
            return Response.StatusCode == HttpStatusCode.OK;
        }

        #endregion

        #region GROUP

        public async Task<string[]> GetFileGroups()
        {
            var uri = string.Format("{0}/v1/groups", Url);
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add("Authorization", BearerToken);
            Response = await httpClient.SendAsync(request, cts.Token);
            ResponseBody = await Response.Content.ReadAsStringAsync();
            if (Response.StatusCode == HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<ValuesResponse>(ResponseBody).Values;
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region FILE

        public async Task<string[]> GetFiles(string group)
        {
            var uri = string.Format("{0}/v1/files/{1}", Url, group);
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add("Authorization", BearerToken);
            Response = await httpClient.SendAsync(request, cts.Token);
            ResponseBody = await Response.Content.ReadAsStringAsync();
            if (Response.StatusCode == HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<ValuesResponse>(ResponseBody).Values;
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region INDEX

        public async Task<PathPositions[]> FindText(string text, SearchOptions option)
        {
            var uri = string.Format("{0}/v1/index/{1}?text={2}&option={3}", Url, GroupName, text, Enum.GetName(option.GetType(), option));
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add("Authorization", BearerToken);
            Response = await httpClient.SendAsync(request, cts.Token);
            ResponseBody = await Response.Content.ReadAsStringAsync();
            if (Response.StatusCode == HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<FindTextResponse>(ResponseBody).Hits;
            }
            else
            {
                return null;
            }
        }

        public async Task<UpdateIndexResponse> UpdateIndex(UpdateIndexRequest input)
        {
            var uri = string.Format("{0}/v1/index/{1}", Url, GroupName);
            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Add("Authorization", BearerToken);
            request.Content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8, "application/json");
            Response = await httpClient.SendAsync(request, cts.Token);
            ResponseBody = await Response.Content.ReadAsStringAsync();
            if (Response.StatusCode == HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<UpdateIndexResponse>(ResponseBody);
            }
            else
            {
                return null;
            }
        }

        public async Task<bool> DeleteIndex()
        {
            var uri = string.Format("{0}/v1/index/{1}", Url, GroupName);
            var request = new HttpRequestMessage(HttpMethod.Delete, uri);
            request.Headers.Add("Authorization", BearerToken);
            Response = await httpClient.SendAsync(request, cts.Token);
            ResponseBody = await Response.Content.ReadAsStringAsync();
            return Response.StatusCode == HttpStatusCode.OK;
        }

        #endregion
    }
}
