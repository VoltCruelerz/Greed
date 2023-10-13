using Greed.Models.Mutations.Variables;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greed.Models.Mutations.Operations.Functions.Strings
{
    public class OpStrConcat : OpFunction
    {
        public OpStrConcat(JObject config) : base(config)
        {
            AssertNParams(3);
        }

        public OpStrConcat(List<Resolvable> parameters) : base(parameters, MutationType.APPEND)
        {
            AssertNParams(3);
        }

        public override object? Exec(JObject root, Dictionary<string, Variable> variables)
        {
            var sb = new StringBuilder();
            Parameters.ForEach(p =>
            {
                if (p == null) return;
                var str = p.Exec(root, variables)?.ToString() ?? "";
                sb.Append(str);
            });
            return sb.ToString();
        }
    }
}
