using Greed.Models.Mutations.Operations.Functions.Comparison;
using Greed.Models.Mutations.Variables;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
