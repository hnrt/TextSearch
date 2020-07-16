using com.hideakin.textsearch.model;
using System;

namespace com.hideakin.textsearch.exception
{
    public class ErrorResponseException : Exception
    {
        public ErrorResponse ErrorResponse { get; }

        public ErrorResponseException(ErrorResponse errorResponse, string message)
            : base(message)
        {
            ErrorResponse = errorResponse;
        }
    }
}
