using System;
using System.Net.Http;
using com.hideakin.textsearch.net;
using com.hideakin.textsearch.utility;

namespace com.hideakin.textsearch.service
{
    public class ServiceBase
    {
        protected ServiceBase()
        {
        }

        public string Url
        {
            get
            {
                return IndexApiClient.Url;
            }
            set
            {
                IndexApiClient.Url = value;
            }
        }

        public string Username
        {
            set
            {
                IndexApiClient.Credentials.Username = value;
            }
        }

        public string Password
        {
            set
            {
                IndexApiClient.Credentials.Password = value;
            }
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
            if (Debug.Level > 0)
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
