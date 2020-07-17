using System;

namespace com.hideakin.textsearch.exception
{
    public class GroupNotExistException : Exception
    {
        public int Gid { get; }

        public GroupNotExistException(int gid)
            : base("Group not exist.")
        {
            Gid = gid;
        }
    }
}
