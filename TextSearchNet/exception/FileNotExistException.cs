using System;

namespace com.hideakin.textsearch.exception
{
    public class FileNotExistException : Exception
    {
        public int Fid { get; }

        public FileNotExistException(int fid)
            : base("File not exist.")
        {
            Fid = fid;
        }
    }
}
