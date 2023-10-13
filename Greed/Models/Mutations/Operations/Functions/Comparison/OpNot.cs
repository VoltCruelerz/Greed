using Greed.Exceptions;
using Greed.Models.Mutations.Variables;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Greed.Models.Mutations.Operations.Functions.Comparison
{
    /// <summary>
    /// Returns the inverse of IsTruthy
    /// </summary>
    public class OpNot : OpFunction
    {
        public OpNot(JObject config) : base(config)
        {
            AssertNParams(1);
        }

        public OpNot(List<Resolvable> parameters) : base(parameters, MutationType.NOT) { }

        public override object? Exec(JObject root, Dictionary<string, Variable> variables)
        {
            return !IsTruthy(Parameters[0], root, variables);
        }
    }
}
