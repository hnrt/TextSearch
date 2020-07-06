using com.hideakin.textsearch.model;
using com.hideakin.textsearch.net;
using System;

namespace com.hideakin.textsearch.service
{
    internal class UserService : ServiceBase
    {
        public UserService()
            : base()
        {
        }

        public UserInfo[] GetUsers()
        {
            var client = new IndexApiClient();
            var task = client.GetUsers();
            task.Wait();
            if (task.Result == null)
            {
                throw NewResponseException(client.Response);
            }
            return task.Result;
        }

        public UserInfo GetUser(int uid)
        {
            var client = new IndexApiClient();
            var task = client.GetUser(uid);
            task.Wait();
            if (task.Result == null)
            {
                throw NewResponseException(client.Response);
            }
            return task.Result;
        }

        public UserInfo GetUser(string username)
        {
            var client = new IndexApiClient();
            var task = client.GetUser(username);
            task.Wait();
            if (task.Result == null)
            {
                throw NewResponseException(client.Response);
            }
            return task.Result;
        }

        public UserInfo CreateUser(string username, string password, string[] roles)
        {
            var client = new IndexApiClient();
            var task = client.CreateUser(username, password, roles);
            task.Wait();
            if (task.Result == null)
            {
                throw NewResponseException(client.Response);
            }
            else if (task.Result is ErrorResponse)
            {
                throw new Exception((task.Result as ErrorResponse).ErrorDescription);
            }
            return task.Result as UserInfo;
        }

        public UserInfo UpdateUser(int uid, string username, string password, string[] roles)
        {
            var client = new IndexApiClient();
            var task = client.UpdateUser(uid, username, password, roles);
            task.Wait();
            if (task.Result == null)
            {
                throw NewResponseException(client.Response);
            }
            else if (task.Result is ErrorResponse)
            {
                throw new Exception((task.Result as ErrorResponse).ErrorDescription);
            }
            return task.Result as UserInfo;
        }

        public UserInfo DeleteUser(int uid)
        {
            var client = new IndexApiClient();
            var task = client.DeleteUser(uid);
            task.Wait();
            if (task.Result == null)
            {
                throw NewResponseException(client.Response);
            }
            else if (task.Result is ErrorResponse)
            {
                throw new Exception((task.Result as ErrorResponse).ErrorDescription);
            }
            return task.Result as UserInfo;
        }
    }
}
