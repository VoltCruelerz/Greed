using Greed.Exceptions;
using Greed.Models.Mutations.Variables;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Greed.Models.Mutations.Operations.Arrays
{
    /// <summary>
    /// Concatenates the Value onto the array.
    /// </summary>
    public class OpArrayAppend : OpArrayInsert
    {
        public OpArrayAppend(JObject obj) : base(obj)
        {
            Value = obj["value"]!;
        }
    }
}
