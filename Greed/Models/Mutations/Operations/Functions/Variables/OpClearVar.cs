using Greed.Models.Mutations.Variables;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greed.Models.Mutations.Operations.Functions.Variables
{
    public class OpClearVar : OpFunction
    {
        public OpClearVar(JObject config) : base(config)
        {
            AssertNParams(1);
        }

        public OpClearVar(List<Resolvable> parameters) : base(parameters, MutationType.SUBSTRING)
        {
            AssertNParams(1);
        }

        public override object? Exec(JObject root, Dictionary<string, Variable> variables)
        {
            var name = Parameters[0].Exec(root, variables)?.ToString();
            if (name == null || !variables.ContainsKey(name)) return null;

            var variable = variables[name];
            variables.Remove(name);

            return variable.Value;
        }
    }
}
