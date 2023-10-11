using System;

namespace Greed.Exceptions
{
    public class MutationParseException : Exception
    {
        public MutationParseException() { }

        public MutationParseException(string message) : base(message) { }

        public MutationParseException(string message, Exception innerException) : base(message, innerException) { }
    }
}
