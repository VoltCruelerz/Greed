using Greed.Models.Mutations.Operations.Primitive;
using System.Collections.Generic;

namespace Greed.Models.Mutations.Variables
{
    public class Variable
    {
        public string Name { get; set; }
        public object? Value { get; set; }

        public int ScopeDepth { get; set; }

        public Variable(string name, object? value, int scopeDepth)
        {
            Name = name;
            Value = value;
            ScopeDepth = scopeDepth;
        }

        public static Dictionary<string, Variable> GetGlobals()
        {
            return new Dictionary<string, Variable>
            {
                { "true", new Variable("true", OpPrimitive.TRUE, -1) },
                { "false", new Variable("false", OpPrimitive.FALSE, -1) },
                { "null", new Variable("null", OpPrimitive.NULL, -1) },
                { "fixed_zero", new Variable("fixed_zero", OpPrimitive.FIXED_ZERO, -1) },
                { "fixed_one", new Variable("fixed_one", OpPrimitive.FIXED_ONE, -1) },
            };
        }
    }
}
