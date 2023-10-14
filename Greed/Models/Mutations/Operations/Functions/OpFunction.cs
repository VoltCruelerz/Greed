using Greed.Exceptions;
using Greed.Extensions;
using Newtonsoft.Json.Linq;
using SharpCompress.Common;
using System.Collections.Generic;
using System.Linq;

namespace Greed.Models.Mutations.Operations.Functions
{
    public abstract class OpFunction : Mutation
    {
        public readonly List<Resolvable> Parameters;

        public OpFunction(JObject config) : base(config)
        {
            var arr = (JArray)config["params"]!;
            Parameters = arr.Select(GenerateResolvable).ToList();
        }

        public void AssertNParams(int count)
        {
            if (Parameters.Count != count)
            {
                throw new ResolvableParseException($"Function type {Type} requires exactly {count} parameters, but you provided {Parameters.Count}.");
            }
        }

        public void AssertAtLeastNParams(int count)
        {
            if (Parameters.Count < count)
            {
                throw new ResolvableParseException($"Function type {Type} requires at lease {count} parameters, but you provided {Parameters.Count}.");
            }
        }

        public OpFunction(List<Resolvable> parameters, MutationType type) : base(type)
        {
            Parameters = parameters;
            if (Parameters.Count == 0)
            {
                throw new ResolvableParseException("You need at least one parameter to perform a function.");
            }
        }

        /// <summary>
        /// Parses a compacted function to the JObject structure we use everywhere else.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static JObject ParseToJObject(string str)
        {
            var parenDiff = str.Count(c => c == '(') - str.Count(c => c == ')');
            if (parenDiff != 0)
            {
                throw new ResolvableParseException("Open and close parentheses count mismatch. Check your JSON.");
            }

            var open = str.IndexOf('(');
            var close = str.LastIndexOf(')');
            var op = str[..open];
            var paramsStr = str.Substring(open + 1, close - 1 - open);

            int paramDepth = 0;
            var arr = new JArray();
            var lastStart = 0;
            for (var i = 0; i < paramsStr.Length; i++)
            {
                var c = paramsStr[i];
                if (c == '(')
                {
                    paramDepth++;
                }
                else if (c == ')')
                {
                    paramDepth--;
                }
                else if (paramDepth == 0 && c == ',')
                {
                    var elStr = paramsStr[lastStart..i];
                    AddParamElement(arr, elStr);
                    lastStart = i + 1;
                }
            }
            AddParamElement(arr, paramsStr[lastStart..]);

            return new JObject
            {
                { "type", op },
                { "params", arr }
            };
        }

        private static void AddParamElement(JArray arr, string elStr)
        {
            elStr = elStr.Trim();
            if (elStr.Contains('('))
            {
                arr.Add(ParseToJObject(elStr));
            }
            else if (bool.TryParse(elStr, out bool parsedBool))
            {
                arr.Add(parsedBool);
            }
            else if (int.TryParse(elStr, out int parsedInt))
            {
                arr.Add(parsedInt);
            }
            else if (double.TryParse(elStr, out double parsedDouble))
            {
                arr.Add(parsedDouble);
            }
            else
            {
                arr.Add(elStr);
            }
        }

        public override string ToString()
        {
            return $"{Type.GetDescription()}({string.Join(",", Parameters.Select(p => p.ToString()))})";
        }
    }
}
