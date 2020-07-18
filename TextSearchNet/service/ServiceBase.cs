using System;
using System.Net.Http;
using System.Threading;
using com.hideakin.textsearch.model;
using com.hideakin.textsearch.net;
using com.hideakin.textsearch.utility;

namespace com.hideakin.textsearch.service
{
    public class ServiceBase
    {
        protected readonly CancellationToken ct;

        protected ServiceBase(CancellationToken ct)
        {
            this.ct = ct;
        }

        public AuthenticateResponse Authenticate(string username, string password)
        {
            using(var client = IndexApiClient.Create(ct))
            {
                var task = client.Authenticate(username, password);
                task.Wait();
                if (task.Result is AuthenticateResponse ar)
                {
                    return ar;
                }
                else if (task.Result is Exception e)
                {
                    throw e;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
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
