using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greed.Models.Mutations
{
    public enum MutationType
    {
        [Description("NONE")]
        NONE = 0,

        [Description("REPLACE")]
        REPLACE = 1
    }
}
