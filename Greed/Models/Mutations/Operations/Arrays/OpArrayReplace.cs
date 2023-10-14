using Newtonsoft.Json.Linq;

namespace Greed.Models.Mutations.Operations.Arrays
{
    /// <summary>
    /// When the condition is met, replace the entry with Value.
    /// </summary>
    public class OpArrayReplace : OpArrayEjectable
    {
        public JToken Value { get; set; }

        public OpArrayReplace(JObject obj) : base(obj)
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
