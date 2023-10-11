using Greed.Exceptions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greed.Models.Mutations.Operations.Logical
{
    public abstract class OpLogical : Mutation
    {
        public readonly List<Resolvable> Parameters;

        public OpLogical(JObject config) : base(config)
        {
            var arr = (JArray)config["params"]!;
            Parameters = arr.Select(GenerateResolvable).ToList();
            if (Parameters.Count == 0)
            {
                throw new ResolvableParseException("You need at least one parameter to perform a logical operation.");
            }
        }
    }
}
