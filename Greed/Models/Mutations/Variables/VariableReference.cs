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
            return variables[Name];
        }
    }
}
