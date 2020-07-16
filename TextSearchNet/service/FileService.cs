using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using com.hideakin.textsearch.data;
using com.hideakin.textsearch.model;
using com.hideakin.textsearch.net;

namespace com.hideakin.textsearch.service
{
    public class FileService : ServiceBase
    {
        private List<(string Path, IndexApiClient Client, Task<object> UploadFileTask)> UploadFileTasks { get; } = new List<(string Path, IndexApiClient Client, Task<object> UploadFileTask)>();

        public int Uploading => UploadFileTasks.Count;

        public FileService(CancellationToken ct)
            : base(ct)
        {
        }

        public FileInfo[] GetFiles(string group)
        {
            var client = IndexApiClient.Create(ct);
            var task = client.GetFiles(group);
            task.Wait();
            if (task.Result is FileInfo[] array)
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

        public FileInfo GetFile(string group, string path)
        {
            var client = IndexApiClient.Create(ct);
            var task = client.GetFile(group, path);
            task.Wait();
            if (task.Result is FileInfo info)
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

        public FileStats GetFileStats(string group)
        {
            var client = IndexApiClient.Create(ct);
            var task = client.GetFileStats(group);
            task.Wait();
            if (task.Result is FileStats stats)
            {
                return stats;
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

        public FileContents DownloadFile(int fid)
        {
            var client = IndexApiClient.Create(ct);
            var task = client.DownloadFile(fid);
            task.Wait();
            if (task.Result is FileContents contents)
            {
                return contents;
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

        public FileInfo UploadFile(string group, string path, out UploadFileStatus result)
        {
            result = UploadFileStatus.Failure;
            var client = IndexApiClient.Create(ct);
            var task = client.UploadFile(group, path);
            task.Wait();
            if (task.Result is FileInfo info)
            {
                result = client.Response.StatusCode == HttpStatusCode.Created ? UploadFileStatus.Created : UploadFileStatus.Updated;
                return info;
            }
            else if (task.Result is Exception e)
            {
                throw new Exception(string.Format("Failed to upload \"{0}\".", path), e);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void UploadFileAsync(string group, string path)
        {
            var client = IndexApiClient.Create(ct);
            var task = client.UploadFile(group, path);
            UploadFileTasks.Add((path, client, task));
        }

        public FileInfo WaitForUploadFileCompletion(out UploadFileStatus result)
        {
            var tasks = new Task<object>[UploadFileTasks.Count];
            int index;
            for (index = 0; index < UploadFileTasks.Count; index++)
            {
                tasks[index] = UploadFileTasks[index].UploadFileTask;
            }
            index = Task.WaitAny(tasks);
            var (path, client, ufTask) = UploadFileTasks[index];
            UploadFileTasks.RemoveAt(index);
            if (ufTask.Result is FileInfo info)
            {
                result = client.Response.StatusCode == HttpStatusCode.Created ? UploadFileStatus.Created : UploadFileStatus.Updated;
                return info;
            }
            else if (ufTask.Result is Exception e)
            {
                throw new Exception(string.Format("Failed to upload \"{0}\".", path), e);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public FileInfo[] DeleteFiles(string group)
        {
            var client = IndexApiClient.Create(ct);
            var task = client.DeleteFiles(group);
            task.Wait();
            if (task.Result is FileInfo[] array)
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

        public void DeleteStaleFiles(string group)
        {
            var client = IndexApiClient.Create(ct);
            var task = client.DeleteStaleFiles(group);
            task.Wait();
            if (task.Result is int)
            {
                return;
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
