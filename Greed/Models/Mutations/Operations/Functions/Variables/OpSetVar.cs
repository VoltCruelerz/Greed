using Greed.Models.Mutations.Variables;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greed.Models.Mutations.Operations.Functions.Variables
{
    public class OpSetVar : OpFunction
    {
        public OpSetVar(JObject config) : base(config)
        {
            AssertNParams(2);
        }

        public OpSetVar(List<Resolvable> parameters) : base(parameters, MutationType.SUBSTRING)
        {
            AssertNParams(2);
        }

        public override object? Exec(JObject root, Dictionary<string, Variable> variables)
        {
            var val = Parameters[1].Exec(root, variables);

            // If we're assigning to a field
            if (Parameters[0] is VariableReference vr) {
                return vr.SetReference(variables, val);
            }
            // If we're assigning to a variable
            else
            {
                var str = Parameters[0].Exec(root, variables)?.ToString();
                if (str == null) return null;
                variables[str] = new Variable(str, val, -1);
                return val;
            }

        }
    }
}
