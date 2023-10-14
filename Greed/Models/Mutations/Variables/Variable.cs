using Newtonsoft.Json.Linq;
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

        public override string ToString()
        {
            return $"[{ScopeDepth}] {Name} := {Value?.ToString()}";
        }

        public static Dictionary<string, Variable> GetGlobals(JObject root)
        {
            return new Dictionary<string, Variable>
            {
                { "root", new Variable("root", root, -1) }
            };
        }
    }
}
