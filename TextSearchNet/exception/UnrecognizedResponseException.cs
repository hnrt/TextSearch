using com.hideakin.textsearch.net;
using System;
using System.Net;

namespace com.hideakin.textsearch.exception
{
    public class UnrecognizedResponseException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public string ResponseBody { get; }

        public string StandardReasonPhrase => HttpReasonPhrase.Get(StatusCode);

        public UnrecognizedResponseException(HttpStatusCode statusCode, string responseBody, string message)
            : base(message)
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }
    }
}
