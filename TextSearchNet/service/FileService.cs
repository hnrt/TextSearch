using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using com.hideakin.textsearch.data;
using com.hideakin.textsearch.exception;
using com.hideakin.textsearch.model;
using com.hideakin.textsearch.net;

namespace com.hideakin.textsearch.service
{
    public class FileService
    {
        private readonly IndexApiClient client;

        private List<(string Path, IndexApiClient Client, Task<object> UploadFileTask)> UploadFileTasks { get; } = new List<(string Path, IndexApiClient Client, Task<object> UploadFileTask)>();

        public int Uploading => UploadFileTasks.Count;

        public FileService(CancellationToken ct)
        {
            client = IndexApiClient.Create(ct);
        }

        public FileInfo[] GetFiles(string group)
        {
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
            var task = client.UploadFile(group, path);
            task.Wait();
            if (task.Result is FileInfo info)
            {
                result = client.StatusCode == HttpStatusCode.Created ? UploadFileStatus.Created : UploadFileStatus.Updated;
                return info;
            }
            else if (task.Result is Exception e)
            {
                throw new UploadFileException(path, e);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void UploadFileAsync(string group, string path, CancellationToken ct)
        {
            var client = IndexApiClient.Create(ct);
            var task = client.UploadFile(group, path);
            UploadFileTasks.Add((path, client, task));
        }

        public FileInfo WaitForUploadFileCompletion(out UploadFileStatus result)
        {
            var tasks = new Task<object>[UploadFileTasks.Count];
            for (int index = 0; index < UploadFileTasks.Count; index++)
            {
                tasks[index] = UploadFileTasks[index].UploadFileTask;
            }
            var completed = Task.WaitAny(tasks);
            var (path, client, task) = UploadFileTasks[completed];
            UploadFileTasks.RemoveAt(completed);
            if (task.Result is FileInfo info)
            {
                result = client.StatusCode == HttpStatusCode.Created ? UploadFileStatus.Created : UploadFileStatus.Updated;
                return info;
            }
            else if (task.Result is Exception e)
            {
                throw new UploadFileException(path, e);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public FileInfo[] DeleteFiles(string group)
        {
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

        public FileInfo[] DeleteStaleFiles(string group)
        {
            var task = client.DeleteStaleFiles(group);
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
    }
}
