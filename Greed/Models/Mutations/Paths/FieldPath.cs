using Greed.Exceptions;
using Greed.Models.Mutations.Variables;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greed.Models.Mutations.Paths
{
    public class FieldPath : ActionPath
    {
        public string Name { get; set; }

        public FieldPath(string name) : base(PathElementEnum.FIELD)
        {
            Name = name;
        }

        public override void DoWork(JToken? token, List<ActionPath> path, int depthRemaining, Dictionary<string, Variable> variables, Action<JToken?, Dictionary<string, Variable>> action)
        {
            if (depthRemaining == 0)
            {
                action(token, variables);
                return;
            }

            if (token == null)
            {
                return;
            }

            if (token is JArray array)
            {
                throw new ResolvableExecException($"Expected value or JObject at action depth {depthRemaining}, but found JArray.");
            }
            else if (token is JObject obj)
            {
                var nextAction = path[^depthRemaining];
                var child = obj[Name];
                nextAction.DoWork(child, path, depthRemaining - 1, variables, action);
            }
            else
            {
                throw new ResolvableExecException($"Unexpected type encountered at action depth {depthRemaining}: {token.GetType()}");
            }
        }
    }
}
