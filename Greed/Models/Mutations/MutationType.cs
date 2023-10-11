using System.ComponentModel;

namespace Greed.Models.Mutations
{
    public enum MutationType
    {
        [Description("NONE")]
        NONE = 0,

        [Description("CONCAT")]
        CONCAT = 1,

        [Description("INSERT")]
        INSERT = 2,

        [Description("FILTER")]
        FILTER = 3,

        [Description("REPLACE")]
        REPLACE = 4,
    }
}
