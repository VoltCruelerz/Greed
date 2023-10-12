using Greed.Exceptions;
using Greed.Models.Mutations.Variables;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Greed.Models.Mutations.Paths
{
    public class ArrayPath : ActionPath
    {
        public string Index { get; set; }
        public string Element { get; set; }
        public ArrayPath(string index) : base(PathElementEnum.ARRAY)
        {
            Index = index;
            Element = "element_" + index;
        }

        public override void DoWork(JToken? token, List<ActionPath> path, int depth, Dictionary<string, Variable> variables, Action<JToken?, Dictionary<string, Variable>, int> action)
        {
            if (depth == path.Count - 1)
            {
                action(token, variables, depth);
                return;
            }

            if (token == null)
            {
                return;
            }

            if (token is JArray array)
            {
                var nextAction = path[depth + 1];
                // Work backwards because deletion is easier.
                for (var i = array.Count - 1; i >= 0; i--)
                {
                    variables.Add(Index, new Variable(Index, i, depth));
                    variables.Add(Element, new Variable(Element, array[i], depth));

                    // I've tried other methods of handling break depths. This is the cleanest option.
                    // Alternatives require OpFilter to basically rewrite all of this exploration code
                    // for itself.
                    try
                    {
                        nextAction.DoWork(array[i], path, depth + 1, variables, action);
                        variables.Remove(Index);
                        variables.Remove(Element);
                    }
                    catch (BreakDepthEjection bde)
                    {
                        bde.TryHandle(depth, array, i, variables);
                    }
                }
                return;
            }

            throw new ResolvableExecException($"Expected JArray at action depth {depth}, but found {token.GetType()}");
        }
    }
}
