using Greed.Extensions;
using Greed.Models.Mutations.Variables;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greed.Models.Mutations.Operations.Primitive
{
    public class OpPrimitive : Mutation
    {
        public static readonly OpPrimitive TRUE       = new(true);
        public static readonly OpPrimitive FALSE      = new(false);
        public static readonly OpPrimitive NULL       = new(null);
        public static readonly OpPrimitive FIXED_ZERO = new(0);
        public static readonly OpPrimitive FIXED_ONE  = new(1);

        public object? Value { get; set; }

        public OpPrimitive(object? value) : base(JObject.Parse($$"""{ "type": "{{MutationType.PRIMITIVE.GetDescription()}}" }"""))
        {
            Value = value;
        }

        public override object? Exec(JObject _, Dictionary<string, Variable> variables)
        {
            return Value;
        }
    }
}
