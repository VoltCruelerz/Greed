using Greed.Exceptions;
using Greed.Extensions;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Greed.Models.Mutations.Variables
{
    public class VariableReference : Resolvable
    {
        /// <summary>
        /// Unlike action paths, which can account for arrays, this CANNOT account for arrays. This can ONLY include fields.
        /// </summary>
        public string[] Path { get; set; }

        public VariableReference(string path)
        {
            if (path.Contains('[') || path.Contains(']'))
            {
                throw new ResolvableParseException("Variable references cannot navigate through arrays. They can only navigate through fields.");
            }
            Path = path.Split(".");
        }

        public override object? Exec(JObject root, Dictionary<string, Variable> variables)
        {
            var terminalNode = GetNodeAtDepth(variables, Path.Length);

            if (terminalNode is JToken token)
            {
                return token.Resolve();
            }

            return terminalNode;
        }

        public object? SetReference(Dictionary<string, Variable> variables, object? value)
        {
            var key = Path[^1];
            var parent = (JToken?)GetTerminalNodeParent(variables);
            if (parent == null) return null;

            if (value == null)
            {
                parent[key] = null;
            }
            else if (value is string str)
            {
                parent[key] = str;
            }
            else if (value is int i)
            {
                parent[key] = i;
            }
            else if (value is long l)
            {
                parent[key] = l;
            }
            else if (value is float f)
            {
                parent[key] = f;
            }
            else if (value is double d)
            {
                parent[key] = d;
            }
            else if (value is JToken token)
            {
                parent[key] = token;
            }
            else
            {
                throw new ResolvableParseException("No handler for dereferencing type " + value.GetType());
            }
            return value;
        }

        private object? GetTerminalNodeParent(Dictionary<string, Variable> variables)
        {
            return GetNodeAtDepth(variables, Path.Length - 1);
        }

        private object? GetNodeAtDepth(Dictionary<string, Variable> variables, int depth)
        {
            var value = variables[Path[0]].Value;
            if (value == null) return null;

            for (int i = 1; i < depth; i++)
            {
                value = ((JObject)value!)[Path[i]];
                if (value == null) return null;
            }

            return value;
        }

        public override string ToString()
        {
            return "(VR$" + string.Join(".", Path) + ")";
        }
    }
}
