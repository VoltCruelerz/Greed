using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greed.Exceptions
{
    public class ModExportException : Exception
    {
        public ModExportException() { }

        public ModExportException(string message) : base(message) { }

        public ModExportException(string message,  Exception innerException) : base(message, innerException) { }
    }
}
