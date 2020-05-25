using com.hideakin.textsearch.data;
using com.hideakin.textsearch.model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public string Url { get; set; } = @"http://localhost:8080";

        public string GroupName { get; set; } = "default";

        private CancellationTokenSource cts = new CancellationTokenSource();

        public IndexNetClient()
        {
            var envUrl = Environment.GetEnvironmentVariable("TEXTINDEXAPI_URL");
            if (envUrl != null)
            {
                 Url= envUrl;
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

        public async Task<DeleteIndexResponse> DeleteIndex()
        {
            var uri = string.Format("{0}/index/{1}", Url, GroupName);
            var request = new HttpRequestMessage(HttpMethod.Delete, uri);
            var response = await httpClient.SendAsync(request, cts.Token);
            var responseBody = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<DeleteIndexResponse>(responseBody);
            }
            else
            {
                return null;
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
    }
}
