using Greed.Exceptions;
using Greed.Models.Mutations.Paths;
using Greed.Models.Mutations.Variables;
using Newtonsoft.Json.Linq;
using SharpCompress.Common;
using System.Collections.Generic;

namespace Greed.Models.Mutations.Operations.Arrays
{
    public class OpArrayIndexOf : OpArray
    {
        public Resolvable Condition { get; set; }

        public OpArrayIndexOf(JObject obj) : base(obj)
        {
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
            int retVal = -1;

            Path[0].DoWork(root, Path, 0, variables, (JToken? token, Dictionary<string, Variable> vars, int depth) =>
            {
                if (token is null) return;

                if (token.GetType() != typeof(JArray)) throw new ResolvableExecException($"Path {string.Join(".", Path)} did not lead to array. Instead, found {token.GetType()}");

                var arr = (JArray)token;

                var lastPath = (ArrayPath)Path[^1];

                for (int i = 0; i < arr.Count; i++)
                {
                    var item = arr[i];

                    // Populate variables
                    variables.Add(lastPath.Index, new Variable(lastPath.Index, i, depth));
                    variables.Add(lastPath.Element, new Variable(lastPath.Element, item, depth));

                    if (IsTruthy(Condition.Exec(root, variables), root, variables))
                    {
                        retVal = i;
                        return;
                    }

                    // Clean up
                    variables.Remove(lastPath.Index);
                    variables.Remove(lastPath.Element);
                }
            });

            return retVal;
        }

        public object? Exec(JObject root)
        {
            return Exec(root, Variable.GetGlobals(root));
        }
    }
}
