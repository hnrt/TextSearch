using com.hideakin.textsearch.model;
using com.hideakin.textsearch.net;
using System;
using System.Threading;

namespace com.hideakin.textsearch.service
{
    public class UserService
    {
        private readonly IndexApiClient client;

        public UserService(CancellationToken ct)
        {
            client = IndexApiClient.Create(ct);
        }

        public UserInfo[] GetUsers()
        {
            var task = client.GetUsers();
            task.Wait();
            if (task.Result is UserInfo[] array)
            {
                return array;
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

        public UserInfo GetUser(int uid)
        {
            var task = client.GetUser(uid);
            task.Wait();
            if (task.Result is UserInfo info)
            {
                return info;
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

        public UserInfo GetUser(string username)
        {
            var task = client.GetUser(username);
            task.Wait();
            if (task.Result is UserInfo info)
            {
                return info;
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

        public UserInfo CreateUser(string username, string password, string[] roles)
        {
            var task = client.CreateUser(username, password, roles);
            task.Wait();
            if (task.Result is UserInfo info)
            {
                return info;
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

        public UserInfo UpdateUser(int uid, string username, string password, string[] roles)
        {
            var task = client.UpdateUser(uid, username, password, roles);
            task.Wait();
            if (task.Result is UserInfo info)
            {
                return info;
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

        public UserInfo DeleteUser(int uid)
        {
            var task = client.DeleteUser(uid);
            task.Wait();
            if (task.Result is UserInfo info)
            {
                return info;
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
