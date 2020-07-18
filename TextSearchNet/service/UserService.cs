using com.hideakin.textsearch.model;
using com.hideakin.textsearch.net;
using System;
using System.Threading;

namespace com.hideakin.textsearch.service
{
    public class UserService : ServiceBase
    {
        public UserService(CancellationToken ct)
            : base(ct)
        {
        }

        public UserInfo[] GetUsers()
        {
            using(var client = IndexApiClient.Create(ct))
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
        }

        public UserInfo GetUser(int uid)
        {
            using(var client = IndexApiClient.Create(ct))
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
        }

        public UserInfo GetUser(string username)
        {
            using(var client = IndexApiClient.Create(ct))
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
        }

        public UserInfo CreateUser(string username, string password, string[] roles)
        {
            using(var client = IndexApiClient.Create(ct))
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
        }

        public UserInfo UpdateUser(int uid, string username, string password, string[] roles)
        {
            using(var client = IndexApiClient.Create(ct))
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
        }

        public UserInfo DeleteUser(int uid)
        {
            using(var client = IndexApiClient.Create(ct))
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
}
