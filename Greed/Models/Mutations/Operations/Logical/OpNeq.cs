using Newtonsoft.Json.Linq;
using Greed.Exceptions;
using System.Linq;
using Greed.Extensions;
using Greed.Models.Mutations.Variables;
using System.Collections.Generic;

namespace Greed.Models.Mutations.Operations.Logical
{
    /// <summary>
    /// Returns TRUE if any of the parameters are not equivalent.
    /// </summary>
    public class OpNeq : OpLogical
    {
        public OpNeq(JObject config) : base(config)
        {
            // Do nothing
        }

        public override object? Exec(JObject root, Dictionary<string, Variable> variables)
        {
            var obj = Parameters[0].Exec(root, variables);
            for (var i = 1; i < Parameters.Count; i++)
            {
                var op = Parameters[i];
                var hypo = op.Exec(root, variables);
                if (obj != hypo)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
