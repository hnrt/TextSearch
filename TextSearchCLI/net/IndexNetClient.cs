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
        private static readonly HttpClient httpClient = new HttpClient();

        public static string Url { get; set; } = @"http://localhost:8080";

        static IndexNetClient()
        {
            var envUrl = Environment.GetEnvironmentVariable("TEXTINDEXAPI_URL");
            if (envUrl != null)
            {
                Url = envUrl;
            }
        }

        public string GroupName { get; set; } = "default";

        private CancellationTokenSource cts = new CancellationTokenSource();

        public HttpResponseMessage Response { get; private set; }

        public string ResponseBody { get; private set; }

        public IndexNetClient()
        {
        }

        public async Task<PathPositions[]> FindText(string text, SearchOptions option)
        {
            var uri = string.Format("{0}/v1/index/{1}?text={2}&option={3}", Url, GroupName, text, Enum.GetName(option.GetType(), option));
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
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
            Response = await httpClient.SendAsync(request, cts.Token);
            ResponseBody = await Response.Content.ReadAsStringAsync();
            return Response.StatusCode == HttpStatusCode.OK;
        }

        public async Task<string> GetPreference(string name)
        {
            var uri = string.Format("{0}/v1/preferences/{1}", Url, name);
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
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
            Response = await httpClient.SendAsync(request, cts.Token);
            ResponseBody = await Response.Content.ReadAsStringAsync();
            return Response.StatusCode == HttpStatusCode.OK;
        }

        public async Task<string[]> GetFileGroups()
        {
            var uri = string.Format("{0}/v1/groups", Url);
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
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

        public async Task<string[]> GetFiles(string group)
        {
            var uri = string.Format("{0}/v1/files/{1}", Url, group);
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
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
    }
}
