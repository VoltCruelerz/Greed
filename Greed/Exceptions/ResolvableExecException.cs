using System;

namespace Greed.Exceptions
{
    public class ResolvableExecException : Exception
    {
        public ResolvableExecException() { }

        public ResolvableExecException(string message) : base(message) { }

        public ResolvableExecException(string message, Exception innerException) : base(message, innerException) { }
    }
}
