using Greed.Exceptions;
using Greed.Models.Mutations.Variables;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Greed.Models.Mutations.Operations.Functions.Comparison.Inequalities
{
    /// <summary>
    /// Returns TRUE if p[0] > p[1]
    /// </summary>
    public class OpGt : OpFunction
    {
        public OpGt(JObject config) : base(config)
        {
            AssertNParams(2);
        }

        public OpGt(List<Resolvable> parameters) : base(parameters, MutationType.GT)
        {
            AssertNParams(2);
        }

        public override object? Exec(JObject root, Dictionary<string, Variable> variables)
        {
            var a = Parameters[0].Exec(root, variables) as int? ?? 0;
            var b = Parameters[1].Exec(root, variables) as int? ?? 0;
            return a > b;
        }
    }
}
