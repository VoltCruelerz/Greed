using System;

namespace Greed.Exceptions
{
    public class MutationExecException : Exception
    {
        public MutationExecException() { }

        public MutationExecException(string message) : base(message) { }

        public MutationExecException(string message, Exception innerException) : base(message, innerException) { }
    }
}
