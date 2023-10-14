using System;

namespace Greed.Exceptions
{
    public class ResolvableParseException : Exception
    {
        public ResolvableParseException() { }

        public ResolvableParseException(string message) : base(message) { }

        public ResolvableParseException(string message, Exception innerException) : base(message, innerException) { }
    }
}
