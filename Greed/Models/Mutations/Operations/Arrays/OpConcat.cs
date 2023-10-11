using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Greed.Exceptions;
using Greed.Models.Mutations.Variables;

namespace Greed.Models.Mutations.Operations.Arrays
{
    /// <summary>
    /// Concatenates the Value onto the array.
    /// </summary>
    public class OpConcat : OpArray
    {
        public JToken Value { get; set; }

        public int BreakDepth { get; set; }

        public OpConcat(JObject obj) : base(obj)
        {
            Value = obj["value"]!;
            BreakDepth = int.Parse(obj["breakDepth"]?.ToString() ?? "0");
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        /// <exception cref="MutationExecException"></exception>
        public override object? Exec(JObject root)
        {
            if (root == null) return null;


            DoWork(root, Path, 0, new Dictionary<string, Variable>(), (JToken token, Dictionary<string, Variable> vars) =>
            {
                if (token.GetType() != typeof(JArray)) throw new MutationExecException($"Path {string.Join(".", Path)} did not lead to array. Instead, found {token.GetType()}");

                var arr = (JArray)token;
                arr.Add(Value);
            });

            return null;
        }
    }
}
