using com.hideakin.textsearch.net;
using System.Collections.Generic;
using System;
using System.Net;
using System.Text;

namespace com.hideakin.textsearch.service
{
    internal class PreferenceService : ServiceBase
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
            var client = new IndexNetClient();
            var task = client.GetPreference(name);
            task.Wait();
            if (client.Response.StatusCode != HttpStatusCode.OK)
            {
                throw NewResponseException(client.Response);
            }
            return task.Result;
        }

        public void UpdatePreference(string name, string value)
        {
            var client = new IndexNetClient();
            var task = client.UpdatePreference(name, value);
            task.Wait();
            if (!task.Result)
            {
                throw NewResponseException(client.Response);
            }
        }

        public void DeletePreference(string name)
        {
            var client = new IndexNetClient();
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
            var extensions = GetPreference(EXTENSIONS);
            if (extensions != null)
            {
                var ss = extensions.Split(new char[] { ',' });
                foreach (string s in ss)
                {
                    list.Add(s.StartsWith(".") ? s : ("." + s));
                }
            }
            return list;
        }

        public void AddExtensions(string extensions)
        {
            var list = GetExtensions();
            var ss = extensions.Split(new char[] { ',', ';', ':' });
            foreach (string s in ss)
            {
                list.Add(s.StartsWith(".") ? s : ("." + s));
            }
            var sb = new StringBuilder();
            foreach (string s in list)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                sb.Append(s);
            }
            if (sb.Length > MAX_LEN)
            {
                throw new Exception(EXTENSIONS + ": Too long.");
            }
            UpdatePreference(EXTENSIONS, sb.ToString());
        }

        public void ClearExtensions()
        {
            DeletePreference(EXTENSIONS);
        }

        public List<string> GetSkipDirs()
        {
            var list = new List<string>();
            var dirs = GetPreference(SKIPDIRS);
            if (dirs != null)
            {
                var ss = dirs.Split(new char[] { ',' });
                foreach (string s in ss)
                {
                    list.Add(s);
                }
            }
            return list;
        }

        public void AddSkipDirs(string dirs)
        {
            var list = GetSkipDirs();
            var ss = dirs.Split(new char[] { ',', ';', ':' });
            foreach (string s in ss)
            {
                list.Add(s);
            }
            var sb = new StringBuilder();
            foreach (string s in list)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                sb.Append(s);
            }
            if (sb.Length > MAX_LEN)
            {
                throw new Exception(SKIPDIRS + ": Too long.");
            }
            UpdatePreference(SKIPDIRS, sb.ToString());
        }

        public void ClearSkipDirs()
        {
            DeletePreference(SKIPDIRS);
        }
    }
}
