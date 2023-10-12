using Greed.Exceptions;
using Greed.Models.Mutations.Operations.Primitive;
using Greed.Models.Mutations.Variables;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Greed.Models.Mutations.Paths;

namespace Greed.Models.Mutations.Operations.Arrays
{
    /// <summary>
    /// Filters the the array to only those where the condition returns true.
    /// </summary>
    public class OpFilter : OpArray
    {
        public int BreakDepth { get; set; }

        public Resolvable Condition { get; set; }

        public OpFilter(JObject obj) : base(obj)
        {
            BreakDepth = obj["breakDepth"]?.Value<int>() ?? Path.Count - 1;
            Condition = GenerateResolvable((JObject)obj["condition"]!);
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

            Path[0].DoWork(root, Path, Path.Count - 1, Variables, (JToken? token, Dictionary<string, Variable> vars) =>
            {
                if (token is null) return;

                if (token.GetType() != typeof(JArray)) throw new ResolvableExecException($"Path {string.Join(".", Path)} did not lead to array. Instead, found {token.GetType()}");

                var arr = (JArray)token;

                var lastPath = (ArrayPath)Path[^1];

                // Loop backwards because deletion is cleaner that way.
                for (int i = arr.Count - 1; i >= 0; i--)
                {
                    var item = arr[i];

                    // Populate variables
                    variables.Add(lastPath.Index, new Variable(lastPath.Index, i));
                    variables.Add(lastPath.Element, new Variable(lastPath.Element, item));

                    // Remove as needed
                    if (!IsTruthy(Condition.Exec(root, variables))) {
                        arr.RemoveAt(i);
                    }

                    // Clean up
                    variables.Remove(lastPath.Index);
                    variables.Remove(lastPath.Element);
                }
            });

            return null;
        }

        public object? Exec(JObject root)
        {
            // Prepopulate with the stock variables.
            var variables = new Dictionary<string, Variable>
            {
                { "true", new Variable("true", OpPrimitive.TRUE) },
                { "false", new Variable("false", OpPrimitive.FALSE) },
                { "null", new Variable("null", OpPrimitive.NULL) },
                { "fixed_zero", new Variable("fixed_zero", OpPrimitive.FIXED_ZERO) },
                { "fixed_one", new Variable("fixed_one", OpPrimitive.FIXED_ONE) },
            };

            return Exec(root, variables);
        }
    }
}
