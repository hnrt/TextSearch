using System;

namespace com.hideakin.textsearch.exception
{
    public class UserNotFoundException : Exception
    {
        public string Username { get; }

        public UserNotFoundException(string username)
            : base("User not found.")
        {
            Username = username;
        }
    }
}
