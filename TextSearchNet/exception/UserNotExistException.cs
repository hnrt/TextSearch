using System;

namespace com.hideakin.textsearch.exception
{
    public class UserNotExistException : Exception
    {
        public int Uid { get; }

        public UserNotExistException(int uid)
            : base("User not exist.")
        {
            Uid = uid;
        }
    }
}
