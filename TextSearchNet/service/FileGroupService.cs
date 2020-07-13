﻿using com.hideakin.textsearch.model;
using com.hideakin.textsearch.net;
using System;

namespace com.hideakin.textsearch.service
{
    public class FileGroupService : ServiceBase
    {
        public FileGroupService()
            : base()
        {
        }

        public FileGroupInfo[] GetFileGroups()
        {
            var client = IndexApiClient.Create();
            var task = client.GetFileGroups();
            task.Wait();
            if (task.Result == null)
            {
                throw NewResponseException(client.Response);
            }
            return task.Result;
        }

        public FileGroupInfo CreateFileGroup(string group)
        {
            var client = IndexApiClient.Create();
            var task = client.CreateFileGroup(group);
            task.Wait();
            if (task.Result == null)
            {
                throw NewResponseException(client.Response);
            }
            else if (task.Result is ErrorResponse)
            {
                throw new Exception((task.Result as ErrorResponse).ErrorDescription);
            }
            return task.Result as FileGroupInfo;
        }

        public FileGroupInfo UpdateFileGroup(int gid, string group)
        {
            var client = IndexApiClient.Create();
            var task = client.UpdateFileGroup(gid, group);
            task.Wait();
            if (task.Result == null)
            {
                throw NewResponseException(client.Response);
            }
            else if (task.Result is ErrorResponse)
            {
                throw new Exception((task.Result as ErrorResponse).ErrorDescription);
            }
            return task.Result as FileGroupInfo;
        }

        public FileGroupInfo DeleteFileGroup(int gid)
        {
            var client = IndexApiClient.Create();
            var task = client.DeleteFileGroup(gid);
            task.Wait();
            if (task.Result == null)
            {
                throw NewResponseException(client.Response);
            }
            else if (task.Result is ErrorResponse)
            {
                throw new Exception((task.Result as ErrorResponse).ErrorDescription);
            }
            return task.Result as FileGroupInfo;
        }
    }
}
