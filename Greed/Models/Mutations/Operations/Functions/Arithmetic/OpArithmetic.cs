using Greed.Models.Mutations.Variables;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Greed.Models.Mutations.Operations.Functions.Comparison
{
    /// <summary>
    /// Returns TRUE if ALL of the parameters resolve to the same value.
    /// </summary>
    public abstract class OpArithmetic : OpFunction
    {
        public OpArithmetic(JObject config) : base(config)
        {
            AssertAtLeastNParams(2);
        }

        public OpArithmetic(List<Resolvable> parameters, MutationType type) : base(parameters, type)
        {
            AssertAtLeastNParams(2);
        }

        public abstract double DoMath(double a, double b);

        public override object? Exec(JObject root, Dictionary<string, Variable> variables)
        {
            var result = ToNumber(Parameters[0].Exec(root, variables), root, variables);
            for (var i = 1; i < Parameters.Count; i++)
            {
                result = DoMath(result, ToNumber(Parameters[i].Exec(root, variables), root, variables));
            }
            return result;
        }
    }
}
