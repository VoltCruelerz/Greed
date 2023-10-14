using Greed.Exceptions;
using Greed.Models.Mutations.Paths;
using Greed.Models.Mutations.Variables;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System;
using static Greed.Models.Mutations.Operations.Arrays.OpArrayFilter;

namespace Greed.Models.Mutations.Operations.Arrays
{
    /// <summary>
    /// Filters the the array to only those where the condition returns true.
    /// </summary>
    public class OpArrayFilter : OpArrayEjectable
    {
        public OpArrayFilter(JObject obj) : base(obj) { }

        public override int Handler(JArray arr, int index)
        {
            arr.RemoveAt(index);
            return arr.Count;
        }
    }
}
