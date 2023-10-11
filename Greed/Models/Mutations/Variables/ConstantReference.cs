using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
