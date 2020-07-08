using System;
using System.Net.Http;
using com.hideakin.textsearch.net;

namespace com.hideakin.textsearch.service
{
    internal class ServiceBase
    {
        protected ServiceBase()
        {
        }

        public void Authenticate()
        {
            var client = new IndexApiClient();
            var task = client.Check();
            task.Wait();
        }

        protected Exception NewResponseException(HttpResponseMessage response)
        {
            return new Exception(string.Format("Status {0}: {1}",
                (int)response.StatusCode,
                string.IsNullOrEmpty(response.ReasonPhrase) ? HttpReasonPhrase.Get(response.StatusCode) : response.ReasonPhrase));
        }

        protected void DebugPut(string header, string input, string[] texts)
        {
            if (Program.DebugLevel > 0)
            {
                if (header != null)
                {
                    Console.WriteLine("#{0}: {1}", header, input);
                }
                else
                {
                    Console.WriteLine("#{0}", input);
                }
                int index = 0;
                foreach (var t in texts)
                {
                    Console.WriteLine("#{0,6} {1}", index++, t);
                }
            }
        }
    }
}
