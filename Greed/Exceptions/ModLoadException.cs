using System;

namespace Greed.Exceptions
{
    public class ModLoadException : Exception
    {
        public ModLoadException() { }

        public ModLoadException(string message) : base(message) { }

        public ModLoadException(string message, Exception innerException) : base(message, innerException) { }
    }
}
