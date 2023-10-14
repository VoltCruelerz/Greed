using Greed.Exceptions;
using Greed.Models.Mutations.Paths;
using Greed.Models.Mutations.Variables;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Greed.Models.Mutations.Operations.Arrays
{
    /// <summary>
    /// Filters the the array to only those where the condition returns true.
    /// </summary>
    public class OpArrayInsert : OpArray
    {
        public int Index { get; set; }
        public JToken Value { get; set; }

        public OpArrayInsert(JObject obj) : base(obj)
        {
            Index = obj["index"]?.Value<int>() ?? -1;
            Value = obj["value"]!;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        /// <exception cref="ResolvableExecException"></exception>
        public override object? Exec(JObject root, Dictionary<string, Variable> variables)
        {
            if (root == null) return null;
            var length = 0;

            Path[0].DoWork(root, Path, 0, variables, (JToken? token, Dictionary<string, Variable> vars, int depth) =>
            {
                if (token is null) return;

                if (token.GetType() != typeof(JArray)) throw new ResolvableExecException($"Path {string.Join(".", Path)} did not lead to array. Instead, found {token.GetType()}");

                var arr = (JArray)token;

                var functionalIndex = Index == -1 ? arr.Count : Index;
                if (int.Clamp(functionalIndex, 0, arr.Count) != functionalIndex)
                {
                    throw new ResolvableExecException($"Index {functionalIndex} was out of the bounds of the array (0 - {arr.Count - 1}. Please review your greed.json");
                }
                arr.Insert(functionalIndex, Value);
                length = arr.Count;
            });

            return length;
        }

        public object? Exec(JObject root)
        {
            return Exec(root, Variable.GetGlobals(root));
        }
    }
}
