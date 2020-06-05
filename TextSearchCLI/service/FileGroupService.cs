using com.hideakin.textsearch.net;

namespace com.hideakin.textsearch.service
{
    internal class FileGroupService : ServiceBase
    {
        public FileGroupService()
            : base()
        {
        }

        public string[] GetFileGroups()
        {
            var client = new IndexNetClient();
            var task = client.GetFileGroups();
            task.Wait();
            if (task.Result == null)
            {
                throw NewResponseException(client.Response);
            }
            return task.Result;
        }
    }
}
