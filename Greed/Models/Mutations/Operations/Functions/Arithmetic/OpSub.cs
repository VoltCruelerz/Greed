using Greed.Models.Mutations.Operations.Functions.Comparison;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Greed.Models.Mutations.Operations.Functions.Arithmetic
{
    public class OpSub : OpArithmetic
    {
        public OpSub(JObject config) : base(config) { }

        public OpSub(List<Resolvable> parameters) : base(parameters, MutationType.SUB) { }

        public override double DoMath(double a, double b)
        {
            return a - b;
        }
    }
}
