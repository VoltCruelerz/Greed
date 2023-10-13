using Greed.Models.Mutations.Variables;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Greed.Models.Mutations.Operations.Functions.Logical
{
    /// <summary>
    /// Returns TRUE if ALL of the parameters are truthy.
    /// </summary>
    public class OpAnd : OpFunction
    {
        public OpAnd(JObject config) : base(config)
        {
            // Do nothing
        }

        public OpAnd(List<Resolvable> parameters) : base(parameters, MutationType.AND) { }

        public override object? Exec(JObject root, Dictionary<string, Variable> variables)
        {
            return Parameters.All(p => IsTruthy(p.Exec(root, variables), root, variables));
        }
    }
}
