﻿using Greed.Exceptions;
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

        public override void DoWork(JToken? token, List<ActionPath> path, int depth, Dictionary<string, Variable> variables, Action<JToken?, Dictionary<string, Variable>> action)
        {
            if (depth == path.Count - 1)
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
                var index = new Variable(Index, 0);
                var element = new Variable(Element, null);
                variables.Add(Index, index);
                variables.Add(Element, element);
                var nextAction = path[depth + 1];
                for (var i = 0; i < array.Count; i++)
                {
                    index.Value = i;
                    element.Value = array[i];
                    nextAction.DoWork(array[i], path, depth + 1, variables, action);
                }
                variables.Remove(Index);
                variables.Remove(Element);
                return;
            }

            throw new ResolvableExecException($"Expected JArray at action depth {depth}, but found {token.GetType()}");
        }
    }
}
