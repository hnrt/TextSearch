using System;

namespace com.hideakin.textsearch.exception
{
    public class GroupFileNotFoundException : Exception
    {
        public string Group { get; }

        public string Path { get; }

        public GroupFileNotFoundException(string group, string path)
            : base("File not found.")
        {
            Group = group;
            Path = path;
        }
    }
}
