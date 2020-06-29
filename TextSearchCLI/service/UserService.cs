using com.hideakin.textsearch.model;
using com.hideakin.textsearch.net;

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
            var client = new IndexNetClient();
            var task = client.GetUsers();
            task.Wait();
            if (task.Result == null)
            {
                throw NewResponseException(client.Response);
            }
            return task.Result;
        }

        public UserInfo GetUser(string username)
        {
            var client = new IndexNetClient();
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
            var client = new IndexNetClient();
            var task = client.CreateUser(username, password, roles);
            task.Wait();
            if (task.Result == null)
            {
                throw NewResponseException(client.Response);
            }
            return task.Result;
        }

        public UserInfo UpdateUser(string username, string password, string[] roles)
        {
            var client = new IndexNetClient();
            var task = client.UpdateUser(username, password, roles);
            task.Wait();
            if (task.Result == null)
            {
                throw NewResponseException(client.Response);
            }
            return task.Result;
        }

        public UserInfo DeleteUser(string username)
        {
            var client = new IndexNetClient();
            var task = client.DeleteUser(username);
            task.Wait();
            if (task.Result == null)
            {
                throw NewResponseException(client.Response);
            }
            return task.Result;
        }
    }
}
