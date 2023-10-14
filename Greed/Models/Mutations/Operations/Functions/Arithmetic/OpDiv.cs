using Greed.Models.Mutations.Operations.Functions.Comparison;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Greed.Models.Mutations.Operations.Functions.Arithmetic
{
    public class OpDiv : OpArithmetic
    {
        public OpDiv(JObject config) : base(config) { }

        public OpDiv(List<Resolvable> parameters) : base(parameters, MutationType.DIV) { }

        public override double DoMath(double a, double b)
        {
            return a / b;
        }
    }
}
