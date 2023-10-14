using Greed.Models.Mutations.Operations.Functions.Comparison;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Greed.Models.Mutations.Operations.Functions.Arithmetic
{
    public class OpAdd : OpArithmetic
    {
        public OpAdd(JObject config) : base(config) { }

        public OpAdd(List<Resolvable> parameters) : base(parameters, MutationType.ADD) { }

        public override double DoMath(double a, double b)
        {
            return a + b;
        }
    }
}
