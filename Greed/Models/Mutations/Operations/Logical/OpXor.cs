using Greed.Models.Mutations.Variables;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Greed.Models.Mutations.Operations.Logical
{
    /// <summary>
    /// Returns TRUE if exactly ONE of the parameters is truthy.
    /// </summary>
    public class OpXor : OpLogical
    {
        public OpXor(JObject config) : base(config)
        {
            // Do nothing
        }

        public override object? Exec(JObject root, Dictionary<string, Variable> variables)
        {
            return 1 == Parameters.Count(p => IsTruthy(p.Exec(root, variables)));
        }
    }
}
