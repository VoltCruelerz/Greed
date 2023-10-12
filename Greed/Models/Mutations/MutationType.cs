using System.ComponentModel;

namespace Greed.Models.Mutations
{
    public enum MutationType
    {
        [Description("NONE")]
        NONE,

        [Description("PRIMITIVE")]
        PRIMITIVE,

        // Logical
        [Description("EQ")]
        EQ,

        [Description("NEQ")]
        NEQ,

        [Description("NOT")]
        NOT,

        [Description("AND")]
        AND,

        [Description("OR")]
        OR,

        [Description("XOR")]
        XOR,

        [Description("LT")]
        LT,

        [Description("GT")]
        GT,

        [Description("LTE")]
        LTE,

        [Description("GTE")]
        GTE,

        // Array Operations
        [Description("CONCAT")]
        CONCAT,

        [Description("INSERT")]
        INSERT,

        [Description("FILTER")]
        FILTER,

        [Description("REPLACE")]
        REPLACE,
    }
}
