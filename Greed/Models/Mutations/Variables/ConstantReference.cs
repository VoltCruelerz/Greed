using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Greed.Models.Mutations.Variables
{
    public class ConstantReference : Resolvable
    {
        public object? Value { get; set; }

        public ConstantReference(object? value)
        {
            Value = value;
        }

        public override object? Exec(JObject root, Dictionary<string, Variable> variables)
        {
            return Value;
        }
    }
}
