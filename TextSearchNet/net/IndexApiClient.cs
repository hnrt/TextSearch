﻿using com.hideakin.textsearch.data;
using com.hideakin.textsearch.model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace com.hideakin.textsearch.net
{
    public class IndexApiClient
    {
        #region CLASS FIELDS

        private static readonly HttpClient httpClient = new HttpClient();

        private static readonly string AUTHORIZATION = "Authorization";

        public static string Url { get; set; } = @"http://localhost:8080";

        private static ApiCreadentialsCollection CredCollection { get; } = ApiCreadentialsCollection.Load(GetFilePath());

        public static ApiCredentials Credentials { get; } = new ApiCredentials();

        #endregion

        #region CLASS INITIALIZER

        static IndexApiClient()
        {
            httpClient.Timeout = Timeout.InfiniteTimeSpan;
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
            if (Credentials.Username != null)
            {
                var c = CredCollection.GetCredentials(Credentials.Username);
                if (c != null)
                {
                    if (Credentials.EncryptedPassword != null)
                    {
                        Credentials.EncryptedPassword = c.EncryptedPassword;
                    }
                    Credentials.EncryptedToken = c.EncryptedToken;
                    Credentials.ExpiresAt = c.ExpiresAt;
                }
            }
            else
            {
                var c = CredCollection.GetCredentials();
                if (c != null)
                {
                    Credentials.Username = c.Username;
                    Credentials.EncryptedPassword = c.EncryptedPassword;
                    Credentials.EncryptedToken = c.EncryptedToken;
                    Credentials.ExpiresAt = c.ExpiresAt;
                }
            }
        }

        public static IndexApiClient Create()
        {
            return new IndexApiClient();
        }

        #endregion

        #region FIELDS

        public HttpResponseMessage Response { get; private set; }

        public string ResponseBody { get; private set; }

        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        public Exception LastException { get; private set; }

        #endregion

        #region CONSTRUCTOR

        private IndexApiClient()
        {
        }

        #endregion

        #region SETUP

        private async Task Initialize()
        {
            using (new IndexApiClientSpinLock())
            {
                if (TokenNeedsToBeUpdated)
                {
                    if (Credentials.Username == null || Credentials.EncryptedPassword == null)
                    {
                        throw new Exception("No valid API key is available. Credentials need to be specified.");
                    }
                    var result = await Authenticate(Credentials.Username, Credentials.Password);
                    if (result is AuthenticateResponse ar)
                    {
                        return;
                    }
                    else if (result is ErrorResponse e)
                    {
                        throw new Exception(e.ErrorDescription);
                    }
                    else
                    {
                        throw new Exception("Unexpected value returned: " + result.GetType().ToString());
                    }
                }
            }
        }

        private bool TokenNeedsToBeUpdated => Credentials.EncryptedToken == null || Credentials.ExpiresAt <= DateTime.Now.AddMinutes(3);

        private static string GetFilePath()
        {
            return Path.Combine(GetDirPath(), string.Format("credentials.json"));
        }

        private static string GetDirPath()
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

        public async Task<object> Authenticate(string username, string password)
        {
            try
            {
                var uri = string.Format("{0}/v1/authentication", Url);
                var request = new HttpRequestMessage(HttpMethod.Get, uri);
                request.Headers.Add(AUTHORIZATION, GetBasicToken(username, password));
                Response = await httpClient.SendAsync(request, cts.Token);
                ResponseBody = await Response.Content.ReadAsStringAsync();
                if (Response.StatusCode == HttpStatusCode.OK)
                {
                    var ar = JsonConvert.DeserializeObject<AuthenticateResponse>(ResponseBody);
                    Credentials.Username = username;
                    Credentials.Password = password;
                    Credentials.AccessToken = ar.AccessToken;
                    Credentials.ExpiresAt = DateTime.Now.AddSeconds(ar.ExpiresIn);
                    CredCollection.SetCredentials(Credentials);
                    CredCollection.LastUser = Credentials.Username;
                    CredCollection.Save(GetFilePath());
                    return ar;
                }
                else
                {
                    return JsonConvert.DeserializeObject<ErrorResponse>(ResponseBody);
                }
            }
            catch (Exception e)
            {
                LastException = e;
                return new ErrorResponse()
                {
                    Error = "api_request_failure",
                    ErrorDescription = e.Message
                };
            }
        }

        private string GetBasicToken(string username, string password)
        {
            var s = string.Format("{0}:{1}", username, password);
            return "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(s));
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
            try
            {
                var uri = string.Format("{0}/v1/maintenance", Url);
                var request = new HttpRequestMessage(HttpMethod.Get, uri);
                Response = await httpClient.SendAsync(request, cts.Token);
                ResponseBody = await Response.Content.ReadAsStringAsync();
                if (Response.StatusCode == HttpStatusCode.OK)
                {
                    return ResponseBody == "false";
                }
            }
            catch (Exception e)
            {
                LastException = e;
            }
            return false;
        }

        #endregion

        #region USER

        public async Task<UserInfo[]> GetUsers()
        {
            try
            {
                await Initialize();
                var uri = string.Format("{0}/v1/users", Url);
                var request = new HttpRequestMessage(HttpMethod.Get, uri);
                request.Headers.Add(AUTHORIZATION, BearerToken);
                Response = await httpClient.SendAsync(request, cts.Token);
                ResponseBody = await Response.Content.ReadAsStringAsync();
                if (Response.StatusCode == HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<UserInfo[]>(ResponseBody);
                }
            }
            catch (Exception e)
            {
                LastException = e;
            }
            return null;
        }

        public async Task<UserInfo> GetUser(int uid)
        {
            try
            {
                await Initialize();
                var uri = string.Format("{0}/v1/users/{1}", Url, uid);
                var request = new HttpRequestMessage(HttpMethod.Get, uri);
                request.Headers.Add(AUTHORIZATION, BearerToken);
                Response = await httpClient.SendAsync(request, cts.Token);
                ResponseBody = await Response.Content.ReadAsStringAsync();
                if (Response.StatusCode == HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<UserInfo>(ResponseBody);
                }
            }
            catch (Exception e)
            {
                LastException = e;
            }
            return null;
        }

        public async Task<UserInfo> GetUser(string username)
        {
            try
            {
                await Initialize();
                var uri = string.Format("{0}/v1/users?username={1}", Url, username);
                var request = new HttpRequestMessage(HttpMethod.Get, uri);
                request.Headers.Add(AUTHORIZATION, BearerToken);
                Response = await httpClient.SendAsync(request, cts.Token);
                ResponseBody = await Response.Content.ReadAsStringAsync();
                if (Response.StatusCode == HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<UserInfo>(ResponseBody);
                }
            }
            catch (Exception e)
            {
                LastException = e;
            }
            return null;
        }

        public async Task<object> CreateUser(string username, string password, string[] roles)
        {
            try
            {
                await Initialize();
                var uri = string.Format("{0}/v1/users", Url);
                var request = new HttpRequestMessage(HttpMethod.Post, uri);
                request.Headers.Add(AUTHORIZATION, BearerToken);
                var input = new UserRequest(username, password, roles);
                request.Content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8, "application/json");
                Response = await httpClient.SendAsync(request, cts.Token);
                ResponseBody = await Response.Content.ReadAsStringAsync();
                if (Response.StatusCode == HttpStatusCode.Created)
                {
                    return JsonConvert.DeserializeObject<UserInfo>(ResponseBody);
                }
                else if (Response.StatusCode == HttpStatusCode.BadRequest || Response.StatusCode == HttpStatusCode.Forbidden)
                {
                    return JsonConvert.DeserializeObject<ErrorResponse>(ResponseBody);
                }
            }
            catch (Exception e)
            {
                LastException = e;
            }
            return null;
        }

        public async Task<object> UpdateUser(int uid, string username, string password, string[] roles)
        {
            try
            {
                await Initialize();
                var uri = string.Format("{0}/v1/users/{1}", Url, uid);
                var request = new HttpRequestMessage(HttpMethod.Put, uri);
                request.Headers.Add(AUTHORIZATION, BearerToken);
                var input = new UserRequest(username, password, roles);
                request.Content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8, "application/json");
                Response = await httpClient.SendAsync(request, cts.Token);
                ResponseBody = await Response.Content.ReadAsStringAsync();
                if (Response.StatusCode == HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<UserInfo>(ResponseBody);
                }
                else if (Response.StatusCode == HttpStatusCode.BadRequest || Response.StatusCode == HttpStatusCode.Forbidden)
                {
                    return JsonConvert.DeserializeObject<ErrorResponse>(ResponseBody);
                }
            }
            catch (Exception e)
            {
                LastException = e;
            }
            return null;
        }

        public async Task<object> DeleteUser(int uid)
        {
            try
            {
                await Initialize();
                var uri = string.Format("{0}/v1/users/{1}", Url, uid);
                var request = new HttpRequestMessage(HttpMethod.Delete, uri);
                request.Headers.Add(AUTHORIZATION, BearerToken);
                Response = await httpClient.SendAsync(request, cts.Token);
                ResponseBody = await Response.Content.ReadAsStringAsync();
                if (Response.StatusCode == HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<UserInfo>(ResponseBody);
                }
                else if (Response.StatusCode == HttpStatusCode.BadRequest || Response.StatusCode == HttpStatusCode.Forbidden)
                {
                    return JsonConvert.DeserializeObject<ErrorResponse>(ResponseBody);
                }
            }
            catch (Exception e)
            {
                LastException = e;
            }
            return null;
        }

        #endregion

        #region GROUP

        public async Task<FileGroupInfo[]> GetFileGroups()
        {
            try
            {
                await Initialize();
                var uri = string.Format("{0}/v1/groups", Url);
                var request = new HttpRequestMessage(HttpMethod.Get, uri);
                request.Headers.Add(AUTHORIZATION, BearerToken);
                Response = await httpClient.SendAsync(request, cts.Token);
                ResponseBody = await Response.Content.ReadAsStringAsync();
                if (Response.StatusCode == HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<FileGroupInfo[]>(ResponseBody);
                }
            }
            catch (Exception e)
            {
                LastException = e;
            }
            return null;
        }

        public async Task<object> CreateFileGroup(string group)
        {
            try
            {
                await Initialize();
                var uri = string.Format("{0}/v1/groups", Url);
                var request = new HttpRequestMessage(HttpMethod.Post, uri);
                request.Headers.Add(AUTHORIZATION, BearerToken);
                var input = new FileGroupRequest(group);
                request.Content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8, "application/json");
                Response = await httpClient.SendAsync(request, cts.Token);
                ResponseBody = await Response.Content.ReadAsStringAsync();
                if (Response.StatusCode == HttpStatusCode.Created)
                {
                    return JsonConvert.DeserializeObject<FileGroupInfo>(ResponseBody);
                }
                else if (Response.StatusCode == HttpStatusCode.BadRequest || Response.StatusCode == HttpStatusCode.Forbidden)
                {
                    return JsonConvert.DeserializeObject<ErrorResponse>(ResponseBody);
                }
            }
            catch (Exception e)
            {
                LastException = e;
            }
            return null;
        }

        public async Task<object> UpdateFileGroup(int gid, string group)
        {
            try
            {
                await Initialize();
                var uri = string.Format("{0}/v1/groups/{1}", Url, gid);
                var request = new HttpRequestMessage(HttpMethod.Put, uri);
                request.Headers.Add(AUTHORIZATION, BearerToken);
                var input = new FileGroupRequest(group);
                request.Content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8, "application/json");
                Response = await httpClient.SendAsync(request, cts.Token);
                ResponseBody = await Response.Content.ReadAsStringAsync();
                if (Response.StatusCode == HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<FileGroupInfo>(ResponseBody);
                }
                else if (Response.StatusCode == HttpStatusCode.BadRequest || Response.StatusCode == HttpStatusCode.Forbidden)
                {
                    return JsonConvert.DeserializeObject<ErrorResponse>(ResponseBody);
                }
            }
            catch (Exception e)
            {
                LastException = e;
            }
            return null;
        }

        public async Task<object> DeleteFileGroup(int gid)
        {
            try
            {
                await Initialize();
                var uri = string.Format("{0}/v1/groups/{1}", Url, gid);
                var request = new HttpRequestMessage(HttpMethod.Delete, uri);
                request.Headers.Add(AUTHORIZATION, BearerToken);
                Response = await httpClient.SendAsync(request, cts.Token);
                ResponseBody = await Response.Content.ReadAsStringAsync();
                if (Response.StatusCode == HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<FileGroupInfo>(ResponseBody);
                }
                else if (Response.StatusCode == HttpStatusCode.Forbidden)
                {
                    return JsonConvert.DeserializeObject<ErrorResponse>(ResponseBody);
                }
            }
            catch (Exception e)
            {
                LastException = e;
            }
            return null;
        }

        #endregion

        #region PREFERENCE

        public async Task<string> GetPreference(string name)
        {
            try
            {
                await Initialize();
                var uri = string.Format("{0}/v1/preferences/{1}", Url, name);
                var request = new HttpRequestMessage(HttpMethod.Get, uri);
                request.Headers.Add(AUTHORIZATION, BearerToken);
                Response = await httpClient.SendAsync(request, cts.Token);
                ResponseBody = await Response.Content.ReadAsStringAsync();
                if (Response.StatusCode == HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<ValueResponse>(ResponseBody).Value;
                }
            }
            catch (Exception e)
            {
                LastException = e;
            }
            return null;
        }

        public async Task<ErrorResponse> SetPreference(string name, string value)
        {
            try
            {
                await Initialize();
                var uri = string.Format("{0}/v1/preferences", Url);
                var request = new HttpRequestMessage(HttpMethod.Post, uri);
                request.Headers.Add(AUTHORIZATION, BearerToken);
                var input = new UpdatePreferenceRequest(name, value);
                request.Content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8, "application/json");
                Response = await httpClient.SendAsync(request, cts.Token);
                ResponseBody = await Response.Content.ReadAsStringAsync();
                if (Response.StatusCode == HttpStatusCode.Created || Response.StatusCode == HttpStatusCode.OK)
                {
                    return null;
                }
                else
                {
                    return JsonConvert.DeserializeObject<ErrorResponse>(ResponseBody);
                }
            }
            catch (Exception e)
            {
                LastException = e;
            }
            return new ErrorResponse()
            {
                Error = "unexpected_error",
                ErrorDescription = "Failed to access Index API."
            };
        }

        public async Task<bool> DeletePreference(string name)
        {
            try
            {
                await Initialize();
                var uri = string.Format("{0}/v1/preferences/{1}", Url, name);
                var request = new HttpRequestMessage(HttpMethod.Delete, uri);
                request.Headers.Add(AUTHORIZATION, BearerToken);
                Response = await httpClient.SendAsync(request, cts.Token);
                ResponseBody = await Response.Content.ReadAsStringAsync();
                return Response.StatusCode == HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                LastException = e;
            }
            return false;
        }

        #endregion

        #region FILE

        public async Task<model.FileInfo[]> GetFiles(string group)
        {
            try
            {
                await Initialize();
                var uri = string.Format("{0}/v1/files/{1}", Url, group);
                var request = new HttpRequestMessage(HttpMethod.Get, uri);
                request.Headers.Add(AUTHORIZATION, BearerToken);
                Response = await httpClient.SendAsync(request, cts.Token);
                ResponseBody = await Response.Content.ReadAsStringAsync();
                if (Response.StatusCode == HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<model.FileInfo[]>(ResponseBody);
                }
            }
            catch (Exception e)
            {
                LastException = e;
            }
            return null;
        }

        public async Task<model.FileInfo> GetFile(string group, string path)
        {
            try
            {
                await Initialize();
                var uri = string.Format("{0}/v1/files/{1}/file?path={2}", Url, group, path);
                var request = new HttpRequestMessage(HttpMethod.Get, uri);
                request.Headers.Add(AUTHORIZATION, BearerToken);
                Response = await httpClient.SendAsync(request, cts.Token);
                ResponseBody = await Response.Content.ReadAsStringAsync();
                if (Response.StatusCode == HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<model.FileInfo>(ResponseBody);
                }
            }
            catch (Exception e)
            {
                LastException = e;
            }
            return null;
        }

        public async Task<FileStats> GetFileStats(string group)
        {
            try
            {
                await Initialize();
                var uri = string.Format("{0}/v1/files/{1}/stats", Url, group);
                var request = new HttpRequestMessage(HttpMethod.Get, uri);
                request.Headers.Add(AUTHORIZATION, BearerToken);
                Response = await httpClient.SendAsync(request, cts.Token);
                ResponseBody = await Response.Content.ReadAsStringAsync();
                if (Response.StatusCode == HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<FileStats>(ResponseBody);
                }
            }
            catch (Exception e)
            {
                LastException = e;
            }
            return null;
        }

        public async Task<model.FileInfo> UploadFile(string group, string path)
        {
            try
            {
                await Initialize();
                var uri = string.Format("{0}/v1/files/{1}", Url, group);
                var request = new HttpRequestMessage(HttpMethod.Post, uri);
                request.Headers.Add(AUTHORIZATION, BearerToken);
                var content = new MultipartFormDataContent();
                var fileContent = new StreamContent(File.OpenRead(path));
                fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "file",
                    FileName = path
                };
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain")
                {
                    CharSet = "UTF-8"
                };
                content.Add(fileContent);
                request.Content = content;
                Response = await httpClient.SendAsync(request, cts.Token);
                ResponseBody = await Response.Content.ReadAsStringAsync();
                if (Response.StatusCode == HttpStatusCode.OK || Response.StatusCode == HttpStatusCode.Created)
                {
                    return JsonConvert.DeserializeObject<model.FileInfo>(ResponseBody);
                }
            }
            catch (Exception e)
            {
                LastException = e;
            }
            return null;
        }

        public async Task<FileContents> DownloadFile(int fid)
        {
            try
            {
                await Initialize();
                var uri = string.Format("{0}/v1/files/{1}/contents", Url, fid);
                var request = new HttpRequestMessage(HttpMethod.Get, uri);
                request.Headers.Add(AUTHORIZATION, BearerToken);
                Response = await httpClient.SendAsync(request, cts.Token);
                if (Response.StatusCode == HttpStatusCode.OK)
                {
                    // NOTE: System.Net.Http.Formatting.Extension NuGet Package needs to be installed to use ReadAsMultipartAsync extension method.
                    var provider = await Response.Content.ReadAsMultipartAsync();
                    foreach (var content in provider.Contents)
                    {
                        var name = RemoveQuotePair(content.Headers.ContentDisposition.Name);
                        if (name == "file")
                        {
                            var encoding = Encoding.GetEncoding(content.Headers.ContentType.CharSet ?? "UTF-8");
                            using (var stream = await content.ReadAsStreamAsync())
                            using (var input = new StreamReader(stream, encoding))
                            {
                                var lines = new List<string>();
                                string line;
                                while ((line = input.ReadLine()) != null)
                                {
                                    lines.Add(line);
                                }
                                return FileContents.Store(fid, RemoveQuotePair(content.Headers.ContentDisposition.FileName), lines.ToArray());
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LastException = e;
            }
            return null;
        }

        public async Task<model.FileInfo[]> DeleteFiles(string group)
        {
            try
            {
                await Initialize();
                var uri = string.Format("{0}/v1/files/{1}", Url, group);
                var request = new HttpRequestMessage(HttpMethod.Delete, uri);
                request.Headers.Add(AUTHORIZATION, BearerToken);
                Response = await httpClient.SendAsync(request, cts.Token);
                ResponseBody = await Response.Content.ReadAsStringAsync();
                if (Response.StatusCode == HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<model.FileInfo[]>(ResponseBody);
                }
            }
            catch (Exception e)
            {
                LastException = e;
            }
            return null;
        }

        public async Task<bool> DeleteStaleFiles(string group)
        {
            try
            {
                await Initialize();
                var uri = string.Format("{0}/v1/files/{1}/stale", Url, group);
                var request = new HttpRequestMessage(HttpMethod.Delete, uri);
                request.Headers.Add(AUTHORIZATION, BearerToken);
                Response = await httpClient.SendAsync(request, cts.Token);
                ResponseBody = await Response.Content.ReadAsStringAsync();
                return Response.StatusCode == HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                LastException = e;
            }
            return false;
        }

        #endregion

        #region INDEX

        public async Task<object> FindText(string group, string text, SearchOptions option)
        {
            try
            {
                await Initialize();
                var uri = string.Format("{0}/v1/index/{1}?text={2}&option={3}", Url, group, text, Enum.GetName(option.GetType(), option));
                var request = new HttpRequestMessage(HttpMethod.Get, uri);
                request.Headers.Add(AUTHORIZATION, BearerToken);
                Response = await httpClient.SendAsync(request, cts.Token);
                ResponseBody = await Response.Content.ReadAsStringAsync();
                if (Response.StatusCode == HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<TextDistribution[]>(ResponseBody);
                }
                else if (Response.StatusCode == HttpStatusCode.BadRequest)
                {
                    return JsonConvert.DeserializeObject<ErrorResponse>(ResponseBody);
                }
            }
            catch (Exception e)
            {
                LastException = e;
            }
            return null;
        }

        #endregion

        #region HELPER

        private static string RemoveQuotePair(string s)
        {
            return (s.Length >= 2 && s[0] == '\"' && s[s.Length - 1] == '\"') ? s.Substring(1, s.Length - 2) : s;
        }

        #endregion
    }
}