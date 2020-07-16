using com.hideakin.textsearch.model;
using com.hideakin.textsearch.net;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace com.hideakin.textsearch.service
{
    public class FileGroupService : ServiceBase
    {
        public FileGroupService(CancellationToken ct)
            : base(ct)
        {
        }

        public FileGroupInfo[] GetFileGroups()
        {
            var client = IndexApiClient.Create(ct);
            var task = client.GetFileGroups();
            task.Wait();
            if (task.Result is FileGroupInfo[] array)
            {
                return array;
            }
            else if (task.Result is Exception exception)
            {
                throw exception;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public async Task<FileGroupInfo[]> GetFileGroupsAsync()
        {
            var client = IndexApiClient.Create(ct);
            var result = await client.GetFileGroups();
            if (result is FileGroupInfo[] array)
            {
                return array;
            }
            else if (result is Exception exception)
            {
                throw exception;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public FileGroupInfo CreateFileGroup(string group)
        {
            var client = IndexApiClient.Create(ct);
            var task = client.CreateFileGroup(group);
            task.Wait();
            if (task.Result is FileGroupInfo info)
            {
                return info;
            }
            else if (task.Result is Exception exception)
            {
                throw exception;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public FileGroupInfo UpdateFileGroup(int gid, string group)
        {
            var client = IndexApiClient.Create(ct);
            var task = client.UpdateFileGroup(gid, group);
            task.Wait();
            if (task.Result is FileGroupInfo info)
            {
                return info;
            }
            else if (task.Result is Exception exception)
            {
                throw exception;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public FileGroupInfo DeleteFileGroup(int gid)
        {
            var client = IndexApiClient.Create(ct);
            var task = client.DeleteFileGroup(gid);
            task.Wait();
            if (task.Result is FileGroupInfo info)
            {
                return info;
            }
            else if (task.Result is Exception exception)
            {
                throw exception;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
