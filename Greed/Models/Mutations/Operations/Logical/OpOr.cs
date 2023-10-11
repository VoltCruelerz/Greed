using Greed.Models.Mutations.Variables;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Greed.Models.Mutations.Operations.Logical
{
    /// <summary>
    /// Returns TRUE if ANY of the parameters are truthy.
    /// </summary>
    public class OpOr : OpLogical
    {
        public OpOr(JObject config) : base(config)
        {
            // Do nothing
        }

        public override object? Exec(JObject root, Dictionary<string, Variable> variables)
        {
            return Parameters.Any(p => IsTruthy(p.Exec(root, variables)));
        }
    }
}
