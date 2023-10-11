using Greed.Exceptions;
using Greed.Models.Mutations.Variables;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Greed.Models.Mutations.Operations.Logical
{
    /// <summary>
    /// Returns the inverse of IsTruthy
    /// </summary>
    public class OpNot : OpLogical
    {
        public OpNot(JObject config) : base(config)
        {
            if (Parameters.Count != 1)
            {
                throw new ResolvableParseException("NOT requires exactly 1 parameter.");
            }
        }

        public override object? Exec(JObject root, Dictionary<string, Variable> variables)
        {
            return !IsTruthy(Parameters[0]);
        }
    }
}
