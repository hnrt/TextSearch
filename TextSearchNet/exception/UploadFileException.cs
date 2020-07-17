using System;

namespace com.hideakin.textsearch.exception
{
    public class UploadFileException : Exception
    {
        public string Path { get; }

        public UploadFileException(string path, Exception innerException)
            : base("UploadFile request failed.", innerException)
        {
            Path = path;
        }
    }
}
