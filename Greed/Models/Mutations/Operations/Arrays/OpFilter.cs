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

            static void handle(JArray arr, int index)
            {
                arr.RemoveAt(index);
            }

            Path[0].DoWork(root, Path, 0, variables, (JToken? token, Dictionary<string, Variable> vars, int depth) =>
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
                    variables.Add(lastPath.Index, new Variable(lastPath.Index, i, depth));
                    variables.Add(lastPath.Element, new Variable(lastPath.Element, item, depth));

                    // Remove as needed
                    if (!IsTruthy(Condition.Exec(root, variables))) {
                        // Try to handle here, but if not, eject until we find the break depth.
                        if (depth == BreakDepth)
                        {
                            handle(arr, i);
                        }
                        else
                        {
                            throw new BreakDepthEjection(handle, BreakDepth);
                        }
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
            return Exec(root, Variable.GetGlobals());
        }
    }
}
