using com.hideakin.textsearch.model;
using com.hideakin.textsearch.net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace com.hideakin.textsearch.service
{
    public class AuthenticationService
    {
        private readonly IndexApiClient client;

        public AuthenticationService(CancellationToken ct)
        {
            client = IndexApiClient.Create(ct);
        }

        public AuthenticateResponse Authenticate(string username, string password)
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
}
