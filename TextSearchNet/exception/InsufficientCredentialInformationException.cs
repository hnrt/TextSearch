using System;

namespace com.hideakin.textsearch.exception
{
    public class InsufficientCredentialInformationException : Exception
    {
        public InsufficientCredentialInformationException(string message)
            : base(message)
        {
        }
    }
}
