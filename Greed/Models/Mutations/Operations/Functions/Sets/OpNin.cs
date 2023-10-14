using Greed.Models.Mutations.Operations.Functions;
using Greed.Models.Mutations.Variables;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Greed.Models.Mutations.Operations.Functions.Sets
{
    /// <summary>
    /// Returns TRUE p[0] is NOT equal to any of the other parameters.
    /// </summary>
    public class OpNin : OpFunction
    {
        public OpNin(JObject config) : base(config)
        {
            // Do nothing
        }

        public OpNin(List<Resolvable> parameters) : base(parameters, MutationType.NIN) { }

        public override object? Exec(JObject root, Dictionary<string, Variable> variables)
        {
            var obj = Parameters[0].Exec(root, variables);
            for (var i = 1; i < Parameters.Count; i++)
            {
                if (AreEqual(obj, Parameters[i].Exec(root, variables)))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
