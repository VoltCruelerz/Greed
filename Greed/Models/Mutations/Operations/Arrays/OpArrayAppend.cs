using Greed.Exceptions;
using Greed.Models.Mutations.Variables;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Greed.Models.Mutations.Operations.Arrays
{
    /// <summary>
    /// Concatenates the Value onto the array.
    /// </summary>
    public class OpArrayAppend : OpArray
    {
        public JToken Value { get; set; }

        public Resolvable? Condition { get; set; }

        public OpArrayAppend(JObject obj) : base(obj)
        {
            Value = obj["value"]!;
            var condition = obj["condition"];
            Condition = condition != null ? GenerateResolvable((JObject)condition) : null;
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
                arr.Add(Value);
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
