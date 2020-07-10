using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using com.hideakin.textsearch.data;
using com.hideakin.textsearch.model;
using com.hideakin.textsearch.net;

namespace com.hideakin.textsearch.service
{
    public class FileService : ServiceBase
    {
        private List<(string Path, IndexApiClient Client, Task<FileInfo> UploadFileTask)> UploadFileTasks { get; } = new List<(string Path, IndexApiClient Client, Task<FileInfo> UploadFileTask)>();

        public int Uploading => UploadFileTasks.Count;

        public FileService()
            : base()
        {
        }

        public FileInfo[] GetFiles(string group)
        {
            var client = new IndexApiClient();
            var task = client.GetFiles(group);
            task.Wait();
            if (task.Result == null)
            {
                throw NewResponseException(client.Response);
            }
            return task.Result;
        }

        public FileInfo GetFile(string group, string path)
        {
            var client = new IndexApiClient();
            var task = client.GetFile(group, path);
            task.Wait();
            if (task.Result == null)
            {
                if (client.Response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new Exception("Path was not found.");
                }
                else
                {
                    throw NewResponseException(client.Response);
                }
            }
            return task.Result;
        }

        public FileStats GetFileStats(string group)
        {
            var client = new IndexApiClient();
            var task = client.GetFileStats(group);
            task.Wait();
            if (task.Result == null)
            {
                throw new Exception("Invalid group.");
            }
            return task.Result;
        }

        public FileContents DownloadFile(int fid)
        {
            var client = new IndexApiClient();
            var task = client.DownloadFile(fid);
            task.Wait();
            if (task.Result == null)
            {
                throw NewResponseException(client.Response);
            }
            return task.Result;
        }

        public FileInfo UploadFile(string group, string path, out UploadFileStatus result)
        {
            result = UploadFileStatus.Failure;
            var client = new IndexApiClient();
            var task = client.UploadFile(group, path);
            task.Wait();
            if (task.Result == null)
            {
                throw new Exception("Failed to upload file " + path);
            }
            result = client.Response.StatusCode == HttpStatusCode.Created ? UploadFileStatus.Created : UploadFileStatus.Updated;
            return task.Result;
        }

        public void AsyncUploadFile(string group, string path)
        {
            var client = new IndexApiClient();
            var task = client.UploadFile(group, path);
            UploadFileTasks.Add((path, client, task));
        }

        public FileInfo WaitForUploadFileCompletion(out UploadFileStatus result)
        {
            var tasks = new Task<FileInfo>[UploadFileTasks.Count];
            int index;
            for (index = 0; index < UploadFileTasks.Count; index++)
            {
                tasks[index] = UploadFileTasks[index].UploadFileTask;
            }
            index = Task.WaitAny(tasks);
            var (path, client, ufTask) = UploadFileTasks[index];
            UploadFileTasks.RemoveAt(index);
            if (ufTask.Result == null)
            {
                throw new Exception("Failed to upload file " + path);
            }
            result = client.Response.StatusCode == HttpStatusCode.Created ? UploadFileStatus.Created : UploadFileStatus.Updated;
            return ufTask.Result;
        }

        public FileInfo[] DeleteFiles(string group)
        {
            var client = new IndexApiClient();
            var task = client.DeleteFiles(group);
            task.Wait();
            if (task.Result == null)
            {
                throw NewResponseException(client.Response);
            }
            return task.Result;
        }

        public void DeleteStaleFiles(string group)
        {
            var client = new IndexApiClient();
            var task = client.DeleteStaleFiles(group);
            task.Wait();
            if (!task.Result)
            {
                throw NewResponseException(client.Response);
            }
        }
    }
}
