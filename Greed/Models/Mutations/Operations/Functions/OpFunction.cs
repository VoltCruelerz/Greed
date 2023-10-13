using Greed.Exceptions;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Greed.Models.Mutations.Operations.Functions
{
    public abstract class OpFunction : Mutation
    {
        public readonly List<Resolvable> Parameters;

        public OpFunction(JObject config) : base(config)
        {
            var arr = (JArray)config["params"]!;
            Parameters = arr.Select(GenerateResolvable).ToList();
        }

        public void AssertNParams(int count)
        {
            if (Parameters.Count != count)
            {
                throw new ResolvableParseException($"Function type {Type} requires exactly {count} parameters, but you provided {Parameters.Count}.");
            }
        }

        public OpFunction(List<Resolvable> parameters, MutationType type) : base(type)
        {
            Parameters = parameters;
            if (Parameters.Count == 0)
            {
                throw new ResolvableParseException("You need at least one parameter to perform a function.");
            }
        }
    }
}
