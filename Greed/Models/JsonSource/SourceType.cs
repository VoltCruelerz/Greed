using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greed.Models.JsonSource
{
    public enum SourceType
    {
        /// <summary>
        /// Overwrite what's there
        /// </summary>
        Replace,

        /// <summary>
        /// Add new bits at the end
        /// </summary>
        Concat
    }
}
