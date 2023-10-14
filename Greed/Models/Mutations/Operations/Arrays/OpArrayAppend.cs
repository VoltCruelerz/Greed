using Newtonsoft.Json.Linq;

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
