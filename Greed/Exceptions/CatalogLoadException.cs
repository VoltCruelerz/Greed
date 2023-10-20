using System;

namespace Greed.Exceptions
{
    public class CatalogLoadException : Exception
    {
        public CatalogLoadException() { }

        public CatalogLoadException(string message) : base(message) { }

        public CatalogLoadException(string message, Exception innerException) : base(message, innerException) { }
    }
}
