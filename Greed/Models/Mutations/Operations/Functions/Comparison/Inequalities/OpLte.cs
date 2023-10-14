using Greed.Exceptions;
using Greed.Models.Mutations.Variables;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Greed.Models.Mutations.Operations.Functions.Comparison.Inequalities
{
    /// <summary>
    /// Returns TRUE if p[0] <= p[1]
    /// </summary>
    public class OpLte : OpFunction
    {
        public OpLte(JObject config) : base(config)
        {
            if (Parameters.Count != 2)
            {
                throw new ResolvableParseException("LTE requires exactly two parameters.");
            }
        }

        public OpLte(List<Resolvable> parameters) : base(parameters, MutationType.LTE)
        {
            AssertNParams(2);
        }

        public override object? Exec(JObject root, Dictionary<string, Variable> variables)
        {
            var a = Parameters[0].Exec(root, variables) as int? ?? 0;
            var b = Parameters[1].Exec(root, variables) as int? ?? 0;
            return a <= b;
        }
    }
}
