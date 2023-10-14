using Greed.Models.Mutations.Variables;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greed.Models.Mutations.Operations.Functions.Strings
{
    public class OpStrSub : OpFunction
    {
        public OpStrSub(JObject config) : base(config)
        {
            AssertNParams(3);
        }

        public OpStrSub(List<Resolvable> parameters) : base(parameters, MutationType.SUBSTRING)
        {
            AssertNParams(3);
        }

        public override object? Exec(JObject root, Dictionary<string, Variable> variables)
        {
            var str = Parameters[0].Exec(root, variables);
            if (str == null) return null;

            var start = Parameters[1].Exec(root, variables);
            var stop = Parameters[2].Exec(root, variables);
            return ((string)str!).Substring((int)(start ?? 0), (int)(stop ?? 0));
        }
    }
}
