using com.hideakin.textsearch.net;
using System.Collections.Generic;
using System;
using System.Net;
using com.hideakin.textsearch.utility;

namespace com.hideakin.textsearch.service
{
    public class PreferenceService : ServiceBase
    {
        private const int MAX_LEN = 8192;

        private static readonly string EXTENSIONS = "extensions";

        private static readonly string SKIPDIRS = "skipdirs";

        public PreferenceService()
            : base()
        {
        }

        public string GetPreference(string name)
        {
            var client = new IndexApiClient();
            var task = client.GetPreference(name);
            task.Wait();
            if (client.Response.StatusCode != HttpStatusCode.OK && client.Response.StatusCode != HttpStatusCode.NotFound)
            {
                throw NewResponseException(client.Response);
            }
            return task.Result;
        }

        public string SetPreference(string name, string value)
        {
            var client = new IndexApiClient();
            var task = client.SetPreference(name, value);
            task.Wait();
            if (task.Result != null)
            {
                throw new Exception(task.Result.ErrorDescription);
            }
            return client.Response.StatusCode == HttpStatusCode.Created ? "Created." : "Updated.";
        }

        public void DeletePreference(string name)
        {
            var client = new IndexApiClient();
            var task = client.DeletePreference(name);
            task.Wait();
            if (!task.Result)
            {
                throw NewResponseException(client.Response);
            }
        }

        public List<string> GetExtensions()
        {
            var list = new List<string>();
            list.MergeItems(GetPreference(EXTENSIONS));
            return list;
        }

        public string AddExtensions(string extensions)
        {
            var list = GetExtensions();
            var csv = list.MergeItems(extensions, s => s.StartsWith(".") ? s : ("." + s)).ToCsvString();
            if (csv.Length > MAX_LEN)
            {
                throw new Exception(EXTENSIONS + ": Too long.");
            }
            return SetPreference(EXTENSIONS, csv);
        }

        public void ClearExtensions()
        {
            DeletePreference(EXTENSIONS);
        }

        public List<string> GetSkipDirs()
        {
            var list = new List<string>();
            list.MergeItems(GetPreference(SKIPDIRS));
            return list;
        }

        public string AddSkipDirs(string dirs)
        {
            var list = GetSkipDirs();
            var csv = list.MergeItems(dirs).ToCsvString();
            if (csv.Length > MAX_LEN)
            {
                throw new Exception(SKIPDIRS + ": Too long.");
            }
            return SetPreference(SKIPDIRS, csv);
        }

        public void ClearSkipDirs()
        {
            DeletePreference(SKIPDIRS);
        }
    }
}
