using com.hideakin.textsearch.net;
using System.Collections.Generic;
using System;
using System.Threading;
using com.hideakin.textsearch.utility;

namespace com.hideakin.textsearch.service
{
    public class PreferenceService
    {
        private const int MAX_LEN = 8192;

        private static readonly string EXTENSIONS = "extensions";

        private static readonly string SKIPDIRS = "skipdirs";

        private readonly IndexApiClient client;

        public PreferenceService(CancellationToken ct)
        {
            client = IndexApiClient.Create(ct);
        }

        public string GetPreference(string name)
        {
            var task = client.GetPreference(name);
            task.Wait();
            if (task.Result is string value)
            {
                return value;
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

        public string SetPreference(string name, string value)
        {
            var task = client.SetPreference(name, value);
            task.Wait();
            if (task.Result is int statusCode)
            {
                return statusCode == 201 ? "Created." : "Updated.";
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

        public void DeletePreference(string name)
        {
            var task = client.DeletePreference(name);
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
