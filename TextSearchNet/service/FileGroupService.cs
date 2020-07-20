using com.hideakin.textsearch.model;
using com.hideakin.textsearch.net;
using System;
using System.Threading;

namespace com.hideakin.textsearch.service
{
    public class FileGroupService
    {
        private readonly IndexApiClient client;

        public FileGroupService(CancellationToken ct)
        {
            client = IndexApiClient.Create(ct);
        }

        public FileGroupInfo[] GetFileGroups()
        {
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

        public FileGroupInfo CreateFileGroup(string group)
        {
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
