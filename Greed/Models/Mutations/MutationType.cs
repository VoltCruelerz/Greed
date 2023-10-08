using System.ComponentModel;

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
