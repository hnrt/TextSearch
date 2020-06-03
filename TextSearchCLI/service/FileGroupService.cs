using com.hideakin.textsearch.net;

namespace com.hideakin.textsearch.service
{
    internal class FileGroupService
    {
        private IndexNetClient NetClient { get; } = IndexNetClient.Instance;

        public FileGroupService()
        {
        }

        public string[] GetFileGroups()
        {
            var task = NetClient.GetFileGroups();
            task.Wait();
            return task.Result;
        }
    }
}
