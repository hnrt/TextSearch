using com.hideakin.textsearch.data;
using com.hideakin.textsearch.model;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace com.hideakin.textsearch.net
{
    internal class IndexNetClient
    {
        public static IndexNetClient Instance { get; } = new IndexNetClient();

        private static readonly HttpClient httpClient = new HttpClient();

        public string Url { get; set; } = @"http://localhost:8080";

        public string GroupName { get; set; } = "default";

        private CancellationTokenSource cts = new CancellationTokenSource();

        private IndexNetClient()
        {
            var envUrl = Environment.GetEnvironmentVariable("TEXTINDEXAPI_URL");
            if (envUrl != null)
            {
                 Url= envUrl;
            }
        }

        public async Task<FindTextResponse> FindText(string text, SearchOptions option)
        {
            var uri = string.Format("{0}/index/{1}?text={2}&option={3}", Url, GroupName, text, Enum.GetName(option.GetType(), option));
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await httpClient.SendAsync(request, cts.Token);
            var responseBody = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<FindTextResponse>(responseBody);
            }
            else
            {
                return null;
            }
        }

        public async Task<UpdateIndexResponse> UpdateIndex(UpdateIndexRequest input)
        {
            var uri = string.Format("{0}/index/{1}", Url, GroupName);
            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8, "application/json");
            var response = await httpClient.SendAsync(request, cts.Token);
            var responseBody = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<UpdateIndexResponse>(responseBody);
            }
            else
            {
                return null;
            }
        }

        public async Task<bool> DeleteIndex()
        {
            var uri = string.Format("{0}/index/{1}", Url, GroupName);
            var request = new HttpRequestMessage(HttpMethod.Delete, uri);
            var response = await httpClient.SendAsync(request, cts.Token);
            var responseBody = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<string> GetPreference(string name)
        {
            var uri = string.Format("{0}/preference/{1}", Url, name);
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await httpClient.SendAsync(request, cts.Token);
            var responseBody = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var rsp = JsonConvert.DeserializeObject<ValueResponse>(responseBody);
                return rsp.Value;
            }
            else
            {
                return null;
            }
        }

        public async Task<bool> UpdatePreference(string name, string value)
        {
            var uri = string.Format("{0}/preference", Url);
            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            var input = new UpdatePreferencesRequest();
            input.Prefs = new NameValuePair[1];
            input.Prefs[0] = new NameValuePair(name, value);
            request.Content = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8, "application/json");
            var response = await httpClient.SendAsync(request, cts.Token);
            var responseBody = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> DeletePreference(string name)
        {
            var uri = string.Format("{0}/preference/{1}", Url, name);
            var request = new HttpRequestMessage(HttpMethod.Delete, uri);
            var response = await httpClient.SendAsync(request, cts.Token);
            var responseBody = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
