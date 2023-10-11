using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Greed.Exceptions;
using Greed.Extensions;
using Greed.Models.Mutations.Variables;
using Greed.Models.Mutations.Operations.Arrays;
using System.Text.RegularExpressions;
using System.Linq;

namespace Greed.Models.Mutations
{
    /// <summary>
    /// This abstract class is designed to handle myriad potential mutations one might make to a JSON object.
    /// </summary>
    public abstract class Mutation
    {
        public MutationType Type { get; set; } = MutationType.NONE;

        public Dictionary<string, Variable> Variables = new Dictionary<string, Variable>();

        public Mutation(JObject obj)
        {
            _ = Enum.TryParse(obj["type"]?.ToString(), out MutationType type);
            Type = type;
        }

        /// <summary>
        /// Executes the mutation on the object.
        /// </summary>
        /// <param name="root">the root of the JSON object file.</param>
        /// <returns></returns>
        public abstract object? Exec(JObject root);

        public static Mutation GenerateMutation(JObject obj)
        {
            var parsed = Enum.TryParse(obj["type"]?.ToString(), out MutationType type);
            if (!parsed)
            {
                throw new MutationParseException("Failed to parse: " + obj.ToString());
            }

            return type switch
            {
                MutationType.NONE => throw new NotImplementedException(),
                MutationType.CONCAT => new OpConcat(obj),
                MutationType.INSERT => throw new NotImplementedException(),
                MutationType.FILTER => throw new NotImplementedException(),
                MutationType.REPLACE => throw new NotImplementedException(),
                _ => throw new MutationParseException("Unrecognized type " + type.GetDescription()),
            };
        }

        public void DoWork(JObject obj, string[] path, int pathIndex, Dictionary<string, Variable> variables, Action<JToken, Dictionary<string, Variable>> action)
        {
            var term = path[pathIndex];
            var child = obj[GetTermAndIndices(term, out List<Variable> indices)];
            indices.ForEach(i => variables.Add(i.Name, i));
            if (child is null)
            {
                return;
            }
            if (path.Length - 1 == pathIndex)
            {
                action(child, variables);
            }
            else if(child is JArray array)
            {
                DoWork(array, path, pathIndex, variables, action);
            }
            else if (child is JObject childObj)
            {
                DoWork(childObj, path, pathIndex + 1, variables, action);
            }
            else
            {
                throw new MutationExecException("Unrecognized type for " + child.ToString());
            }
            indices.ForEach(i => variables.Remove(i.Name));
        }

        public void DoWork(JArray arr, string[] path, int i, Dictionary<string, Variable> variables, Action<JToken, Dictionary<string, Variable>> action)
        {
            foreach (var item in arr)
            {
                if (item is JArray subArray)
                {
                    DoWork(subArray, path, i, variables, action);
                }
                else if (path.Length - 1 == i)
                {
                    action(item, variables);
                }
                else if (item is JObject subObject)
                {
                    DoWork(subObject, path, i + 1, variables, action);
                }
                else
                {
                    throw new MutationExecException("Unrecognized type for " + item.ToString());
                }
            }
        }

        /// <summary>
        /// Provided "a[i][j][k]", returns "a" and sets indices to a list containing three Variables set to 0.
        /// </summary>
        /// <param name="term">The initial term, eg "a[i]"</param>
        /// <param name="indices">Set to a list of variables containing the index variables</param>
        /// <returns>The name of the field</returns>
        private static string GetTermAndIndices(string term, out List<Variable> indices)
        {
            indices = new List<Variable>();

            var iteratorTerms = term
                .Replace("]", "")
                .Split("[")
                .ToArray();

            for (int i = 0; i < iteratorTerms.Length; i++)
            {
                var iTerm = iteratorTerms[i];
                if (i == 0)
                {
                    term = iTerm;
                }
                else
                {
                    indices.Add(new Variable(iTerm, 0));
                }
            }

            return term;
        }
    }
}
