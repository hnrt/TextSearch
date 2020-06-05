using com.hideakin.textsearch.net;

namespace com.hideakin.textsearch.service
{
    internal class FileService : ServiceBase
    {
        public FileService()
            : base()
        {
        }

        public string[] GetFiles(string group)
        {
            var client = new IndexNetClient() { GroupName = group };
            var task = client.GetFiles(group);
            task.Wait();
            if (task.Result == null)
            {
                throw NewResponseException(client.Response);
            }
            return task.Result;
        }
    }
}
