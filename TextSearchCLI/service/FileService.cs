using com.hideakin.textsearch.net;

namespace com.hideakin.textsearch.service
{
    internal class FileService
    {
        private IndexNetClient NetClient { get; } = IndexNetClient.Instance;

        public FileService()
        {
        }

        public string[] GetFiles(string group)
        {
            var task = NetClient.GetFiles(group);
            task.Wait();
            return task.Result;
        }
    }
}
