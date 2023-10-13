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

        // Inequalities
        [Description("LT")]
        LT,
        [Description("GT")]
        GT,
        [Description("LTE")]
        LTE,
        [Description("GTE")]
        GTE,

        // Sets
        [Description("IN")]
        IN,
        [Description("NIN")]
        NIN,

        // Strings
        [Description("SUBSTRING")]
        SUBSTRING,
        [Description("CONCAT")]
        CONCAT,

        // Variables
        [Description("SET")]
        SET,
        [Description("CLEAR")]
        CLEAR,

        // Array Operations
        [Description("APPEND")]
        APPEND,
        [Description("INSERT")]
        INSERT,
        [Description("FILTER")]
        FILTER,
        [Description("REPLACE")]
        REPLACE,
        [Description("DISTINCT")]
        DISTINCT,
    }
}
