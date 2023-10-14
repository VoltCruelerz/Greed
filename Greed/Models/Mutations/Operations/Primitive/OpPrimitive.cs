using Greed.Extensions;
using Greed.Models.Mutations.Variables;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Greed.Models.Mutations.Operations.Primitive
{
    public class OpPrimitive : Mutation
    {
        public object? Value { get; set; }

        public OpPrimitive(object? value) : base(JObject.Parse($$"""{ "type": "{{MutationType.PRIMITIVE.GetDescription()}}" }"""))
        {
            Value = value;
        }

        public override object? Exec(JObject _, Dictionary<string, Variable> variables)
        {
            return Value;
        }

        public override string ToString()
        {
            return $"Primitive({Value})";
        }
    }
}
