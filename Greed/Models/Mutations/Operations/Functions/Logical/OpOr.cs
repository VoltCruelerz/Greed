using Greed.Models.Mutations.Variables;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Greed.Models.Mutations.Operations.Functions.Logical
{
    /// <summary>
    /// Returns TRUE if ANY of the parameters are truthy.
    /// </summary>
    public class OpOr : OpFunction
    {
        public OpOr(JObject config) : base(config)
        {
            // Do nothing
        }

        public OpOr(List<Resolvable> parameters) : base(parameters, MutationType.OR) { }

        public override object? Exec(JObject root, Dictionary<string, Variable> variables)
        {
            return Parameters.Any(p => IsTruthy(p.Exec(root, variables), root, variables));
        }
    }
}
