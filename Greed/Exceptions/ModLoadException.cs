using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greed.Exceptions
{
    public class ModLoadException : Exception
    {
        public ModLoadException() { }

        public ModLoadException(string message) : base(message) { }

        public ModLoadException(string message, Exception innerException) : base(message, innerException) { }
    }
}
