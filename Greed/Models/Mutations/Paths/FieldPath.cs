using Greed.Exceptions;
using Greed.Models.Mutations.Variables;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Greed.Models.Mutations.Paths
{
    public class FieldPath : ActionPath
    {
        public string Name { get; set; }

        public FieldPath(string name) : base(PathElementEnum.FIELD)
        {
            Name = name;
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
                throw new ResolvableExecException($"Expected value or JObject at action depth {depth}, but found JArray.");
            }
            else if (token is JObject obj)
            {
                var nextAction = path[depth + 1];
                var child = obj[Name];
                nextAction.DoWork(child, path, depth + 1, variables, action);
            }
            else
            {
                throw new ResolvableExecException($"Unexpected type encountered at action depth {depth}: {token.GetType()}");
            }
        }
    }
}
