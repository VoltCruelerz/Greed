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
