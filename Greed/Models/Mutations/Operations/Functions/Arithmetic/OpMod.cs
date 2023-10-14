using Greed.Models.Mutations.Operations.Functions.Comparison;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Greed.Models.Mutations.Operations.Functions.Arithmetic
{
    public class OpMod : OpArithmetic
    {
        public OpMod(JObject config) : base(config) { }

        public OpMod(List<Resolvable> parameters) : base(parameters, MutationType.DIV) { }

        public override double DoMath(double a, double b)
        {
            return a % b;
        }
    }
}
