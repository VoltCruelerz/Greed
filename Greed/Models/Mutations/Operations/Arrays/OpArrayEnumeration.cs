using Greed.Exceptions;
using Greed.Models.Mutations.Paths;
using Greed.Models.Mutations.Variables;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System;
using static Greed.Models.Mutations.Operations.Arrays.OpArrayFilter;

namespace Greed.Models.Mutations.Operations.Arrays
{
    /// <summary>
    /// When the criteria is met (or not), execute the implementing class's handler.
    /// </summary>
    public abstract class OpArrayEnumeration : OpArray
    {
        public int ResolutionDepth { get; set; }

        public Resolvable Condition { get; set; }

        /// <summary>
        /// If TRUE, executes the handler when the constraint is violated. On FALSE, it executes when the constraint is fulfilled.
        /// </summary>
        public bool ExecuteOnViolation = true;

        public OpArrayEnumeration(JObject obj) : base(obj)
        {
            ResolutionDepth = obj["resolutionDepth"]?.Value<int>() ?? Path.Count - 1;
            Condition = GenerateResolvable((JObject)obj["condition"]!);
        }

        public abstract int Handler(JArray arr, int index);

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
            var capsule = new BreakDepthEjection.BreakCapsule();

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

                    var constraintResult = IsTruthy(Condition.Exec(root, variables), root, variables);
                    var needToHandle = ExecuteOnViolation ? !constraintResult : constraintResult;

                    // Handle as needed
                    if (needToHandle)
                    {
                        // Try to handle here, but if not, eject until we find the break depth.
                        if (depth == ResolutionDepth)
                        {
                            length = Handler(arr, i);
                        }
                        else
                        {
                            throw new BreakDepthEjection(Handler, ResolutionDepth, capsule);
                        }
                    }

                    // Clean up
                    variables.Remove(lastPath.Index);
                    variables.Remove(lastPath.Element);
                }
            });

            return capsule.Value;
        }

        public object? Exec(JObject root)
        {
            return Exec(root, Variable.GetGlobals(root));
        }
    }
}
