using com.hideakin.textsearch.net;
using System.Collections.Generic;
using System;
using System.Text;

namespace com.hideakin.textsearch.service
{
    internal class PreferenceService
    {
        private const int MAX_LEN = 8192;

        private static readonly string EXTENSIONS = "extensions";

        private static readonly string SKIPDIRS = "skipdirs";

        private IndexNetClient NetClient { get; } = IndexNetClient.Instance;

        public PreferenceService()
        {
        }

        public string GetPreference(string name)
        {
            var task = NetClient.GetPreference(name);
            task.Wait();
            return task.Result;
        }

        public bool UpdatePreference(string name, string value)
        {
            var task = NetClient.UpdatePreference(name, value);
            task.Wait();
            return task.Result;
        }

        public bool DeletePreference(string name)
        {
            var task = NetClient.DeletePreference(name);
            task.Wait();
            return task.Result;
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

        public bool AddExtensions(string extensions)
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
            return UpdatePreference(EXTENSIONS, sb.ToString());
        }

        public bool ClearExtensions()
        {
            return DeletePreference(EXTENSIONS);
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

        public bool AddSkipDirs(string dirs)
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
            return UpdatePreference(SKIPDIRS, sb.ToString());
        }

        public bool ClearSkipDirs()
        {
            return DeletePreference(SKIPDIRS);
        }
    }
}
