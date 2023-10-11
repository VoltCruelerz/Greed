using Greed.Models.Mutations.Variables;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Greed.Exceptions;

namespace Greed.Models.Mutations.Paths
{
    public abstract class ActionPath
    {
        private const string ValidationStr = @"^[a-zA-Z_0-9]+?((?<field>\.[a-zA-Z_]+?)|(?<arr>\[[a-zA-Z_]*?\]))*$";
        private static readonly Regex Validator = new(ValidationStr);
        public PathElementEnum PathElement { get; set; }

        public ActionPath(PathElementEnum type)
        {
            PathElement = type;
        }

        public abstract void DoWork(JToken? token, List<ActionPath> path, int depthRemaining, Dictionary<string, Variable> variables, Action<JToken?, Dictionary<string, Variable>> action);

        public static List<ActionPath> Build(string path)
        {
            var ap = new List<ActionPath>();

            if (!Validator.IsMatch(path))
            {
                throw new ResolvableParseException($"Failed to parse {path}\nExpected it to meet the following regular expression:\n{ValidationStr}");
            }

            var terms = path.Split(".");
            foreach (var term in terms)
            {
                var subTerms = term.Replace("]", "").Split("[").ToArray();
                ap.Add(new FieldPath(subTerms[0]));

                for (var i = 1; i < subTerms.Length; i++)
                {
                    var subTerm = subTerms[i];
                    ap.Add(new ArrayPath(subTerm));
                }
            }

            return ap;
        }
    }
}
