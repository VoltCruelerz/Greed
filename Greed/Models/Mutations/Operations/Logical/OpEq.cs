using Greed.Models.Mutations.Variables;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Greed.Models.Mutations.Operations.Logical
{
    /// <summary>
    /// Returns TRUE if ALL of the parameters resolve to the same value.
    /// </summary>
    public class OpEq : OpLogical
    {
        public OpEq(JObject config) : base(config)
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
                if (AreEqual(obj, hypo))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
