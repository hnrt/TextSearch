using com.hideakin.textsearch.data;
using com.hideakin.textsearch.exception;
using com.hideakin.textsearch.model;
using com.hideakin.textsearch.utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
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

        private static readonly string APPLICATION_JSON = "application/json";

        private static readonly string CONNECTION = "Connection";

        private static readonly string CLOSE = "close";

        private const int MAX_ATTEMPTS = 1000;

        public static string Url { get; set; } = @"http://localhost:8080";

        private static string CredentialsFilePath => AppData.GetPath("credentials.json");

        private static ApiCreadentialsCollection CredCollection { get; } = ApiCreadentialsCollection.Load(CredentialsFilePath);

        public static ApiCredentials Credentials { get; } = new ApiCredentials();

        #endregion

        #region CLASS INITIALIZER / METHODS

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
            if (Credentials.Username != null && Credentials.EncryptedPassword == null)
            {
                ChangeUser(Credentials.Username);
            }
            else
            {
                ChangeUser();
            }
        }

        public static bool ChangeUser(string username = null)
        {
            var c = username != null ? CredCollection.GetCredentials(username) : CredCollection.GetCredentials();
            if (c != null)
            {
                Credentials.Username = c.Username;
                Credentials.EncryptedPassword = c.EncryptedPassword;
                Credentials.EncryptedToken = c.EncryptedToken;
                Credentials.ExpiresAt = c.ExpiresAt;
                if (username != null && username != CredCollection.LastUser)
                {
                    CredCollection.LastUser = c.Username;
                    CredCollection.Save(CredentialsFilePath);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public static IndexApiClient Create(CancellationToken ct)
        {
            return new IndexApiClient(ct);
        }

        #endregion

        #region FIELDS

        public HttpStatusCode StatusCode { get; private set; }

        private readonly CancellationToken ct;

        #endregion

        #region CONSTRUCTOR

        private IndexApiClient(CancellationToken ct)
        {
            this.ct = ct;
        }

        #endregion

        #region SETUP

        private async Task Initialize()
        {
            using (new IndexApiClientSpinLock())
            {
                if (Credentials.EncryptedToken == null || Credentials.ExpiresAt <= DateTime.Now.AddMinutes(3))
                {
                    if (Credentials.Username == null)
                    {
                        throw new InsufficientCredentialInformationException("No username is provided for authentication.");
                    }
                    if (Credentials.EncryptedPassword == null)
                    {
                        throw new InsufficientCredentialInformationException("No password is provided for authentication.");
                    }
                    var result = await Authenticate(Credentials.Username, Credentials.Password);
                    if (result is AuthenticateResponse ar)
                    {
                        return;
                    }
                    else if (result is Exception e)
                    {
                        throw e;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }
        }

        #endregion

        #region AUTHENTICATION

        public async Task<object> Authenticate(string username, string password)
        {
            try
            {
                var uri = string.Format("{0}/v1/authentication", Url);
                using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                {
                    request.Headers.Add(AUTHORIZATION, GetBasicToken(username, password));
                    using (var response = await httpClient.SendAsync(request, ct))
                    {
                        StatusCode = response.StatusCode;
                        var responseBody = await response.Content.ReadAsStringAsync();
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            var ar = JsonConvert.DeserializeObject<AuthenticateResponse>(responseBody);
                            Credentials.Username = username;
                            Credentials.Password = password;
                            Credentials.AccessToken = ar.AccessToken;
                            Credentials.ExpiresAt = DateTime.Now.AddSeconds(ar.ExpiresIn);
                            CredCollection.SetCredentials(Credentials);
                            CredCollection.LastUser = Credentials.Username;
                            CredCollection.Save(CredentialsFilePath);
                            return ar;
                        }
                        else
                        {
                            return new ErrorResponseException(JsonConvert.DeserializeObject<ErrorResponse>(responseBody), "Authentication request failed.");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return e;
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

        public async Task<object> GetMaintenance()
        {
            try
            {
                var uri = string.Format("{0}/v1/maintenance", Url);
                using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                {
                    using (var response = await httpClient.SendAsync(request, ct))
                    {
                        StatusCode = response.StatusCode;
                        var responseBody = await response.Content.ReadAsStringAsync();
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            return responseBody == "true";
                        }
                        else
                        {
                            return new UnrecognizedResponseException(response.StatusCode, responseBody, "GetMaintenance request failed.");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return e;
            }
        }

        #endregion

        #region USER

        public async Task<object> GetUsers()
        {
            try
            {
                await Initialize();
                var uri = string.Format("{0}/v1/users", Url);
                using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                {
                    request.Headers.Add(AUTHORIZATION, BearerToken);
                    using (var response = await httpClient.SendAsync(request, ct))
                    {
                        StatusCode = response.StatusCode;
                        var responseBody = await response.Content.ReadAsStringAsync();
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            return JsonConvert.DeserializeObject<UserInfo[]>(responseBody);
                        }
                        else if (response.StatusCode == HttpStatusCode.Forbidden)
                        {
                            return new ErrorResponseException(JsonConvert.DeserializeObject<ErrorResponse>(responseBody), "GetUsers request failed.");
                        }
                        else
                        {
                            return new UnrecognizedResponseException(response.StatusCode, responseBody, "GetUsers request failed.");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return e;
            }
        }

        public async Task<object> GetUser(int uid)
        {
            try
            {
                await Initialize();
                var uri = string.Format("{0}/v1/users/{1}", Url, uid);
                using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                {
                    request.Headers.Add(AUTHORIZATION, BearerToken);
                    using (var response = await httpClient.SendAsync(request, ct))
                    {
                        StatusCode = response.StatusCode;
                        var responseBody = await response.Content.ReadAsStringAsync();
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            return JsonConvert.DeserializeObject<UserInfo>(responseBody);
                        }
                        else if (response.StatusCode == HttpStatusCode.Forbidden)
                        {
                            return new ErrorResponseException(JsonConvert.DeserializeObject<ErrorResponse>(responseBody), "GetUserByUID request failed.");
                        }
                        else if (response.StatusCode == HttpStatusCode.NotFound)
                        {
                            return new UserNotExistException(uid);
                        }
                        else
                        {
                            return new UnrecognizedResponseException(response.StatusCode, responseBody, "GetUserByUID request failed.");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return e;
            }
        }

        public async Task<object> GetUser(string username)
        {
            try
            {
                await Initialize();
                var uri = string.Format("{0}/v1/users?username={1}", Url, username);
                using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                {
                    request.Headers.Add(AUTHORIZATION, BearerToken);
                    using (var response = await httpClient.SendAsync(request, ct))
                    {
                        StatusCode = response.StatusCode;
                        var responseBody = await response.Content.ReadAsStringAsync();
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            return JsonConvert.DeserializeObject<UserInfo>(responseBody);
                        }
                        else if (response.StatusCode == HttpStatusCode.Forbidden)
                        {
                            return new ErrorResponseException(JsonConvert.DeserializeObject<ErrorResponse>(responseBody), "GetUserByName request failed.");
                        }
                        else if (response.StatusCode == HttpStatusCode.NotFound)
                        {
                            return new UserNotFoundException(username);
                        }
                        else
                        {
                            return new UnrecognizedResponseException(response.StatusCode, responseBody, "GetUserByName request failed.");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return e;
            }
        }

        public async Task<object> CreateUser(string username, string password, string[] roles)
        {
            try
            {
                await Initialize();
                var uri = string.Format("{0}/v1/users", Url);
                using (var request = new HttpRequestMessage(HttpMethod.Post, uri))
                {
                    request.Headers.Add(AUTHORIZATION, BearerToken);
                    var input = new UserRequest(username, password, roles);
                    request.Content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8, APPLICATION_JSON);
                    using (var response = await httpClient.SendAsync(request, ct))
                    {
                        StatusCode = response.StatusCode;
                        var responseBody = await response.Content.ReadAsStringAsync();
                        if (response.StatusCode == HttpStatusCode.Created)
                        {
                            return JsonConvert.DeserializeObject<UserInfo>(responseBody);
                        }
                        else if (response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.Forbidden)
                        {
                            return new ErrorResponseException(JsonConvert.DeserializeObject<ErrorResponse>(responseBody), "CreateUser request failed.");
                        }
                        else
                        {
                            return new UnrecognizedResponseException(response.StatusCode, responseBody, "CreateUser request failed.");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return e;
            }
        }

        public async Task<object> UpdateUser(int uid, string username, string password, string[] roles)
        {
            try
            {
                await Initialize();
                var uri = string.Format("{0}/v1/users/{1}", Url, uid);
                using (var request = new HttpRequestMessage(HttpMethod.Put, uri))
                {
                    request.Headers.Add(AUTHORIZATION, BearerToken);
                    var input = new UserRequest(username, password, roles);
                    request.Content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8, APPLICATION_JSON);
                    using (var response = await httpClient.SendAsync(request, ct))
                    {
                        StatusCode = response.StatusCode;
                        var responseBody = await response.Content.ReadAsStringAsync();
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            return JsonConvert.DeserializeObject<UserInfo>(responseBody);
                        }
                        else if (response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.Forbidden)
                        {
                            return new ErrorResponseException(JsonConvert.DeserializeObject<ErrorResponse>(responseBody), "UpdateUser request failed.");
                        }
                        else if (response.StatusCode == HttpStatusCode.NotFound)
                        {
                            return new UserNotExistException(uid);
                        }
                        else
                        {
                            return new UnrecognizedResponseException(response.StatusCode, responseBody, "UpdateUser request failed.");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return e;
            }
        }

        public async Task<object> DeleteUser(int uid)
        {
            try
            {
                await Initialize();
                var uri = string.Format("{0}/v1/users/{1}", Url, uid);
                using (var request = new HttpRequestMessage(HttpMethod.Delete, uri))
                {
                    request.Headers.Add(AUTHORIZATION, BearerToken);
                    using (var response = await httpClient.SendAsync(request, ct))
                    {
                        StatusCode = response.StatusCode;
                        var responseBody = await response.Content.ReadAsStringAsync();
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            return JsonConvert.DeserializeObject<UserInfo>(responseBody);
                        }
                        else if (response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.Forbidden)
                        {
                            return new ErrorResponseException(JsonConvert.DeserializeObject<ErrorResponse>(responseBody), "DeleteUser request failed.");
                        }
                        else if (response.StatusCode == HttpStatusCode.NotFound)
                        {
                            return new UserNotExistException(uid);
                        }
                        else
                        {
                            return new UnrecognizedResponseException(response.StatusCode, responseBody, "DeleteUser request failed.");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return e;
            }
        }

        #endregion

        #region GROUP

        public async Task<object> GetFileGroups()
        {
            try
            {
                await Initialize();
                var uri = string.Format("{0}/v1/groups", Url);
                using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                {
                    request.Headers.Add(AUTHORIZATION, BearerToken);
                    using (var response = await httpClient.SendAsync(request, ct))
                    {
                        StatusCode = response.StatusCode;
                        var responseBody = await response.Content.ReadAsStringAsync();
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            return JsonConvert.DeserializeObject<FileGroupInfo[]>(responseBody);
                        }
                        else
                        {
                            return new UnrecognizedResponseException(response.StatusCode, responseBody, "GetFileGroups request failed.");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return e;
            }
        }

        public async Task<object> CreateFileGroup(string group)
        {
            try
            {
                await Initialize();
                var uri = string.Format("{0}/v1/groups", Url);
                using (var request = new HttpRequestMessage(HttpMethod.Post, uri))
                {
                    request.Headers.Add(AUTHORIZATION, BearerToken);
                    var input = new FileGroupRequest(group);
                    request.Content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8, APPLICATION_JSON);
                    using (var response = await httpClient.SendAsync(request, ct))
                    {
                        StatusCode = response.StatusCode;
                        var responseBody = await response.Content.ReadAsStringAsync();
                        if (response.StatusCode == HttpStatusCode.Created)
                        {
                            return JsonConvert.DeserializeObject<FileGroupInfo>(responseBody);
                        }
                        else if (response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.Forbidden)
                        {
                            return new ErrorResponseException(JsonConvert.DeserializeObject<ErrorResponse>(responseBody), "CreateFileGroup request failed.");
                        }
                        else
                        {
                            return new UnrecognizedResponseException(response.StatusCode, responseBody, "CreateFileGroup request failed.");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return e;
            }
        }

        public async Task<object> UpdateFileGroup(int gid, string group)
        {
            try
            {
                await Initialize();
                var uri = string.Format("{0}/v1/groups/{1}", Url, gid);
                using (var request = new HttpRequestMessage(HttpMethod.Put, uri))
                {
                    request.Headers.Add(AUTHORIZATION, BearerToken);
                    var input = new FileGroupRequest(group);
                    request.Content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8, APPLICATION_JSON);
                    using (var response = await httpClient.SendAsync(request, ct))
                    {
                        StatusCode = response.StatusCode;
                        var responseBody = await response.Content.ReadAsStringAsync();
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            return JsonConvert.DeserializeObject<FileGroupInfo>(responseBody);
                        }
                        else if (response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.Forbidden)
                        {
                            return new ErrorResponseException(JsonConvert.DeserializeObject<ErrorResponse>(responseBody), "UpdateFileGroup request failed.");
                        }
                        else if (response.StatusCode == HttpStatusCode.NotFound)
                        {
                            return new GroupNotExistException(gid);
                        }
                        else
                        {
                            return new UnrecognizedResponseException(response.StatusCode, responseBody, "UpdateFileGroup request failed.");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return e;
            }
        }

        public async Task<object> DeleteFileGroup(int gid)
        {
            try
            {
                await Initialize();
                var uri = string.Format("{0}/v1/groups/{1}", Url, gid);
                using (var request = new HttpRequestMessage(HttpMethod.Delete, uri))
                {
                    request.Headers.Add(AUTHORIZATION, BearerToken);
                    using (var response = await httpClient.SendAsync(request, ct))
                    {
                        StatusCode = response.StatusCode;
                        var responseBody = await response.Content.ReadAsStringAsync();
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            return JsonConvert.DeserializeObject<FileGroupInfo>(responseBody);
                        }
                        else if (response.StatusCode == HttpStatusCode.Forbidden)
                        {
                            return new ErrorResponseException(JsonConvert.DeserializeObject<ErrorResponse>(responseBody), "DeleteFileGroup request failed.");
                        }
                        else if (response.StatusCode == HttpStatusCode.NotFound)
                        {
                            return new GroupNotExistException(gid);
                        }
                        else
                        {
                            return new UnrecognizedResponseException(response.StatusCode, responseBody, "DeleteFileGroup request failed.");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return e;
            }
        }

        #endregion

        #region PREFERENCE

        public async Task<object> GetPreference(string name)
        {
            try
            {
                await Initialize();
                var uri = string.Format("{0}/v1/preferences/{1}", Url, name);
                using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                {
                    request.Headers.Add(AUTHORIZATION, BearerToken);
                    using (var response = await httpClient.SendAsync(request, ct))
                    {
                        StatusCode = response.StatusCode;
                        var responseBody = await response.Content.ReadAsStringAsync();
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            return JsonConvert.DeserializeObject<ValueResponse>(responseBody).Value;
                        }
                        else
                        {
                            return new UnrecognizedResponseException(response.StatusCode, responseBody, "GetPreference request failed.");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return e;
            }
        }

        public async Task<object> SetPreference(string name, string value)
        {
            try
            {
                await Initialize();
                var uri = string.Format("{0}/v1/preferences", Url);
                using (var request = new HttpRequestMessage(HttpMethod.Post, uri))
                {
                    request.Headers.Add(AUTHORIZATION, BearerToken);
                    var input = new UpdatePreferenceRequest(name, value);
                    request.Content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8, APPLICATION_JSON);
                    using (var response = await httpClient.SendAsync(request, ct))
                    {
                        StatusCode = response.StatusCode;
                        var responseBody = await response.Content.ReadAsStringAsync();
                        if (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK)
                        {
                            return (int)StatusCode;
                        }
                        else
                        {
                            return new ErrorResponseException(JsonConvert.DeserializeObject<ErrorResponse>(responseBody), "SetPreference request failed.");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return e;
            }
        }

        public async Task<object> DeletePreference(string name)
        {
            try
            {
                await Initialize();
                var uri = string.Format("{0}/v1/preferences/{1}", Url, name);
                using (var request = new HttpRequestMessage(HttpMethod.Delete, uri))
                {
                    request.Headers.Add(AUTHORIZATION, BearerToken);
                    using (var response = await httpClient.SendAsync(request, ct))
                    {
                        StatusCode = response.StatusCode;
                        var responseBody = await response.Content.ReadAsStringAsync();
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            return (int)StatusCode;
                        }
                        else if (response.StatusCode == HttpStatusCode.Forbidden)
                        {
                            return new ErrorResponseException(JsonConvert.DeserializeObject<ErrorResponse>(responseBody), "FindText request failed.");
                        }
                        else
                        {
                            return new UnrecognizedResponseException(response.StatusCode, responseBody, "DeletePreference request failed.");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return e;
            }
        }

        #endregion

        #region FILE

        public async Task<object> GetFiles(string group)
        {
            try
            {
                await Initialize();
                var uri = string.Format("{0}/v1/files/{1}", Url, group);
                using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                {
                    request.Headers.Add(AUTHORIZATION, BearerToken);
                    using (var response = await httpClient.SendAsync(request, ct))
                    {
                        StatusCode = response.StatusCode;
                        var responseBody = await response.Content.ReadAsStringAsync();
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            return JsonConvert.DeserializeObject<model.FileInfo[]>(responseBody);
                        }
                        else if (response.StatusCode == HttpStatusCode.NotFound)
                        {
                            return new GroupNotFoundException(group);
                        }
                        else
                        {
                            return new UnrecognizedResponseException(response.StatusCode, responseBody, "GetFiles request failed.");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return e;
            }
        }

        public async Task<object> GetFile(string group, string path)
        {
            try
            {
                await Initialize();
                var uri = string.Format("{0}/v1/files/{1}/file?path={2}", Url, group, path);
                using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                {
                    request.Headers.Add(AUTHORIZATION, BearerToken);
                    using (var response = await httpClient.SendAsync(request, ct))
                    {
                        StatusCode = response.StatusCode;
                        var responseBody = await response.Content.ReadAsStringAsync();
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            return JsonConvert.DeserializeObject<model.FileInfo>(responseBody);
                        }
                        else if (response.StatusCode == HttpStatusCode.NotFound)
                        {
                            return new GroupFileNotFoundException(group, path);
                        }
                        else
                        {
                            return new UnrecognizedResponseException(response.StatusCode, responseBody, "GetFile request failed.");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return e;
            }
        }

        public async Task<object> GetFileStats(string group)
        {
            try
            {
                await Initialize();
                var uri = string.Format("{0}/v1/files/{1}/stats", Url, group);
                using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                {
                    request.Headers.Add(AUTHORIZATION, BearerToken);
                    using (var response = await httpClient.SendAsync(request, ct))
                    {
                        StatusCode = response.StatusCode;
                        var responseBody = await response.Content.ReadAsStringAsync();
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            return JsonConvert.DeserializeObject<FileStats>(responseBody);
                        }
                        else if (response.StatusCode == HttpStatusCode.NotFound)
                        {
                            return new GroupNotFoundException(group);
                        }
                        else
                        {
                            return new UnrecognizedResponseException(response.StatusCode, responseBody, "GetFileStats request failed.");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return e;
            }
        }

        public async Task<object> UploadFile(string group, string path)
        {
            try
            {
                await Initialize();
                var uri = string.Format("{0}/v1/files/{1}", Url, group);
                using (var request = new HttpRequestMessage(HttpMethod.Post, uri))
                {
                    request.Headers.Add(AUTHORIZATION, BearerToken);
                    request.Headers.Add(CONNECTION, CLOSE);
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
                    using (var response = await httpClient.SendAsync(request, ct))
                    {
                        StatusCode = response.StatusCode;
                        var responseBody = await response.Content.ReadAsStringAsync();
                        if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
                        {
                            return JsonConvert.DeserializeObject<model.FileInfo>(responseBody);
                        }
                        else if (response.StatusCode == HttpStatusCode.Forbidden)
                        {
                            return new ErrorResponseException(JsonConvert.DeserializeObject<ErrorResponse>(responseBody), "UploadFile request failed.");
                        }
                        else if (response.StatusCode == HttpStatusCode.NotFound)
                        {
                            return new GroupNotFoundException(group);
                        }
                        else
                        {
                            return new UnrecognizedResponseException(response.StatusCode, responseBody, "UploadFile request failed.");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return e;
            }
        }

        public async Task<object> DownloadFile(int fid)
        {
            for (int attempt = 1; ; attempt++)
            {
                try
                {
                    await Initialize();
                    var uri = string.Format("{0}/v1/files/{1}/contents", Url, fid);
                    using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                    {
                        request.Headers.Add(AUTHORIZATION, BearerToken);
                        request.Headers.Add(CONNECTION, CLOSE);
                        using (var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct))
                        {
                            StatusCode = response.StatusCode;
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                // NOTE: System.Net.Http.Formatting.Extension NuGet Package needs to be installed to use ReadAsMultipartAsync extension method.
                                var provider = await response.Content.ReadAsMultipartAsync();
                                foreach (var content in provider.Contents)
                                {
                                    using (content)
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
                            else if (response.StatusCode == HttpStatusCode.NotFound)
                            {
                                return new FileNotExistException(fid);
                            }
                            return new UnrecognizedResponseException(StatusCode, "", "DownloadFile request failed.");
                        }
                    }
                }
                catch (HttpRequestException e) when(e.InnerException is WebException ei && ei.InnerException is SocketException eii && eii.SocketErrorCode == SocketError.AccessDenied && attempt < MAX_ATTEMPTS)
                {
#if DEBUG
                    Console.WriteLine("#DownloadFile({0})@{1} Access denied. Retrying...", fid, attempt);
#endif
                    Thread.Sleep(10);
                    continue;
                }
                catch (IOException e) when(e.InnerException is IOException ei && ei.InnerException is SocketException eii && eii.SocketErrorCode == SocketError.ConnectionAborted && attempt < MAX_ATTEMPTS)
                {
#if DEBUG
                    Console.WriteLine("#DownloadFile({0})@{1} Connection aborted. Retrying...", fid, attempt);
#endif
                    Thread.Sleep(10);
                    continue;
                }
                catch (IOException e) when (e.InnerException is IOException ei && ei.InnerException is SocketException eii && eii.SocketErrorCode == SocketError.ConnectionReset && attempt < MAX_ATTEMPTS)
                {
#if DEBUG
                    Console.WriteLine("#DownloadFile({0})@{1} Connection reset. Retrying...", fid, attempt);
#endif
                    Thread.Sleep(10);
                    continue;
                }
                catch (Exception e)
                {
#if DEBUG
                    Console.WriteLine("#DownloadFile({0})@{1} Returning {2}...", fid, attempt, e.GetType().FullName);
#endif
                    return e;
                }
            }
        }

        public async Task<object> DeleteFiles(string group)
        {
            try
            {
                await Initialize();
                var uri = string.Format("{0}/v1/files/{1}", Url, group);
                using (var request = new HttpRequestMessage(HttpMethod.Delete, uri))
                {
                    request.Headers.Add(AUTHORIZATION, BearerToken);
                    using (var response = await httpClient.SendAsync(request, ct))
                    {
                        StatusCode = response.StatusCode;
                        var responseBody = await response.Content.ReadAsStringAsync();
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            return JsonConvert.DeserializeObject<model.FileInfo[]>(responseBody);
                        }
                        else if (response.StatusCode == HttpStatusCode.Forbidden)
                        {
                            return new ErrorResponseException(JsonConvert.DeserializeObject<ErrorResponse>(responseBody), "DeleteStaleFiles request failed.");
                        }
                        else if (response.StatusCode == HttpStatusCode.NotFound)
                        {
                            return new GroupNotFoundException(group);
                        }
                        else
                        {
                            return new UnrecognizedResponseException(response.StatusCode, responseBody, "DeleteFiles request failed.");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return e;
            }
        }

        public async Task<object> DeleteStaleFiles(string group)
        {
            try
            {
                await Initialize();
                var uri = string.Format("{0}/v1/files/{1}/stale", Url, group);
                using (var request = new HttpRequestMessage(HttpMethod.Delete, uri))
                {
                    request.Headers.Add(AUTHORIZATION, BearerToken);
                    using (var response = await httpClient.SendAsync(request, ct))
                    {
                        StatusCode = response.StatusCode;
                        var responseBody = await response.Content.ReadAsStringAsync();
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            return (int)StatusCode;
                        }
                        else if (response.StatusCode == HttpStatusCode.Forbidden)
                        {
                            return new ErrorResponseException(JsonConvert.DeserializeObject<ErrorResponse>(responseBody), "DeleteStaleFiles request failed.");
                        }
                        else if (response.StatusCode == HttpStatusCode.NotFound)
                        {
                            return new GroupNotFoundException(group);
                        }
                        else
                        {
                            return new UnrecognizedResponseException(response.StatusCode, responseBody, "DeleteStaleFiles request failed.");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return e;
            }
        }

        #endregion

        #region INDEX

        public async Task<object> FindText(string group, string text, SearchOptions option)
        {
            try
            {
                await Initialize();
                var uri = string.Format("{0}/v1/index/{1}?text={2}&option={3}", Url, group, text, Enum.GetName(option.GetType(), option));
                using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                {
                    request.Headers.Add(AUTHORIZATION, BearerToken);
                    using (var response = await httpClient.SendAsync(request, ct))
                    {
                        StatusCode = response.StatusCode;
                        var responseBody = await response.Content.ReadAsStringAsync();
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            return JsonConvert.DeserializeObject<TextDistribution[]>(responseBody);
                        }
                        else if (response.StatusCode == HttpStatusCode.BadRequest)
                        {
                            return new ErrorResponseException(JsonConvert.DeserializeObject<ErrorResponse>(responseBody), "FindText request failed.");
                        }
                        else if (response.StatusCode == HttpStatusCode.NotFound)
                        {
                            return new GroupNotFoundException(group);
                        }
                        else
                        {
                            return new UnrecognizedResponseException(response.StatusCode, responseBody, "FindText request failed.");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return e;
            }
        }

        public async Task<object> FindText(string group, string text, SearchOptions option, int limit, int offset)
        {
            try
            {
                await Initialize();
                var uri = string.Format("{0}/v2/index/{1}?text={2}&option={3}&limit={4}&offset={5}", Url, group, text, Enum.GetName(option.GetType(), option), limit, offset);
                using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                {
                    request.Headers.Add(AUTHORIZATION, BearerToken);
                    using (var response = await httpClient.SendAsync(request, ct))
                    {
                        StatusCode = response.StatusCode;
                        var responseBody = await response.Content.ReadAsStringAsync();
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            return JsonConvert.DeserializeObject<TextDistribution[]>(responseBody);
                        }
                        else if (response.StatusCode == HttpStatusCode.BadRequest)
                        {
                            return new ErrorResponseException(JsonConvert.DeserializeObject<ErrorResponse>(responseBody), "FindTextV2 request failed.");
                        }
                        else if (response.StatusCode == HttpStatusCode.NotFound)
                        {
                            return new GroupNotFoundException(group);
                        }
                        else
                        {
                            return new UnrecognizedResponseException(response.StatusCode, responseBody, "FindTextV2 request failed.");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return e;
            }
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
