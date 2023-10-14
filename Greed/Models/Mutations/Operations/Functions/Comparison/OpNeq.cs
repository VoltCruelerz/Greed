using Greed.Models.Mutations.Variables;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Greed.Models.Mutations.Operations.Functions.Comparison
{
    /// <summary>
    /// Returns TRUE if the parameters are not equivalent.
    /// </summary>
    public class OpNeq : OpFunction
    {
        public OpNeq(JObject config) : base(config)
        {
            AssertNParams(2);
        }

        public OpNeq(List<Resolvable> parameters) : base(parameters, MutationType.NEQ)
        {
            AssertNParams(2);
        }

        public override object? Exec(JObject root, Dictionary<string, Variable> variables)
        {
            var a = Parameters[0].Exec(root, variables);
            var b = Parameters[1].Exec(root, variables);
            return !AreEqual(a, b);
        }
    }
}
