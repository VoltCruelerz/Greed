using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
