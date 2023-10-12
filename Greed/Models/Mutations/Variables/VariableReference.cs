using Greed.Extensions;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Greed.Models.Mutations.Variables
{
    public class VariableReference : Resolvable
    {
        public string Name { get; set; }

        public VariableReference(string name)        {
            Name = name;
        }

        public override object? Exec(JObject root, Dictionary<string, Variable> variables)
        {
            var value = variables[Name].Value;
            if (value == null) return null;

            if (value is JToken token)
            {
                return token.Resolve();
            }

            return value;
        }
    }
}
