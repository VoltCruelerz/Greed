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

        public VariableReference(string path)        {
            Path = path.Split(".");
        }

        public override object? Exec(JObject root, Dictionary<string, Variable> variables)
        {
            var value = variables[Path[0]].Value;
            if (value == null) return null;

            for (int i = 1; i < Path.Length; i++)
            {
                value = ((JObject)value!)[Path[i]];
                if (value == null) return null;
            }

            if (value is JToken token)
            {
                return token.Resolve();
            }

            return value;
        }

        public override string ToString()
        {
            return "$" + string.Join(".", Path);
        }
    }
}
