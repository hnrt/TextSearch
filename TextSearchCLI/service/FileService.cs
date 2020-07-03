﻿using System;
using System.Net;
using com.hideakin.textsearch.data;
using com.hideakin.textsearch.model;
using com.hideakin.textsearch.net;

namespace com.hideakin.textsearch.service
{
    internal class FileService : ServiceBase
    {
        public FileService()
            : base()
        {
        }

        public FileInfo[] GetFiles(string group)
        {
            var client = new IndexNetClient();
            var task = client.GetFiles(group);
            task.Wait();
            if (task.Result == null)
            {
                throw NewResponseException(client.Response);
            }
            return task.Result;
        }

        public FileStats GetFileStats(string group)
        {
            var client = new IndexNetClient();
            var task = client.GetFileStats(group);
            task.Wait();
            if (task.Result == null)
            {
                throw new Exception("Invalid group.");
            }
            return task.Result;
        }

        public string[] DownloadFile(int fid)
        {
            var client = new IndexNetClient();
            var task = client.DownloadFile(fid);
            task.Wait();
            if (task.Result == null)
            {
                throw NewResponseException(client.Response);
            }
            return task.Result;
        }

        public model.FileInfo UploadFile(string group, string path, out UploadFileStatus result)
        {
            result = UploadFileStatus.Failure;
            var client = new IndexNetClient();
            var task = client.UploadFile(group, path);
            task.Wait();
            if (task.Result == null)
            {
                throw new Exception("Failed to upload file " + path);
            }
            result = client.Response.StatusCode == HttpStatusCode.Created ? UploadFileStatus.Created : UploadFileStatus.Updated;
            return task.Result;
        }
    }
}
