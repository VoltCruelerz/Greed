using System;

namespace Greed.Exceptions
{
    public class DiffException : Exception
    {
        public DiffException() { }

        public DiffException(string message) : base(message) { }

        public DiffException(string message, Exception innerException) : base(message, innerException) { }
    }
}
