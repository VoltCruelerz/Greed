using Greed.Exceptions;
using Greed.Models.Mutations.Paths;
using Greed.Models.Mutations.Variables;
using Newtonsoft.Json.Linq;
using SharpCompress.Common;
using System.Collections.Generic;

namespace Greed.Models.Mutations.Operations.Arrays
{
    public class OpArrayIndexOf : OpArrayEnumeration
    {
        public JToken Value { get; set; }

        public OpArrayIndexOf(JObject obj) : base(obj)
        {
            Value = obj["value"]!;
            ExecuteOnViolation = false;
        }

        public override int Handler(JArray arr, int index)
        {
            arr[index] = Value;
            return arr.Count;
        }
    }
}
