using System;

namespace Greed.Exceptions
{
    public class ModExportException : Exception
    {
        public ModExportException() { }

        public ModExportException(string message) : base(message) { }

        public ModExportException(string message, Exception innerException) : base(message, innerException) { }
    }
}
