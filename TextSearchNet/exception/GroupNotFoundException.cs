using System;

namespace com.hideakin.textsearch.exception
{
    public class GroupNotFoundException : Exception
    {
        public string Group { get; }

        public GroupNotFoundException(string group)
            : base("Group not found.")
        {
            Group = group;
        }
    }
}
