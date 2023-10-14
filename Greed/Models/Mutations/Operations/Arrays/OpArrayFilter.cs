using Newtonsoft.Json.Linq;

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
