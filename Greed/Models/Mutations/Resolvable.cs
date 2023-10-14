using Greed.Exceptions;
using Greed.Extensions;
using Greed.Models.Mutations.Operations.Arrays;
using Greed.Models.Mutations.Operations.Functions.Logical;
using Greed.Models.Mutations.Operations.Functions.Comparison;
using Greed.Models.Mutations.Operations.Functions.Comparison.Inequalities;
using Greed.Models.Mutations.Operations.Functions.Sets;
using Greed.Models.Mutations.Operations.Functions.Strings;
using Greed.Models.Mutations.Variables;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Greed.Models.Mutations
{
    /// <summary>
    /// Resolvables are expressions that resolve to a value.
    /// </summary>
    public abstract class Resolvable
    {
        /// <summary>
        /// Executes the resolvable on the object and variable set.
        /// </summary>
        /// <param name="root">the root of the JSON object file.</param>
        /// <returns></returns>
        public abstract object? Exec(JObject root, Dictionary<string, Variable> variables);

        public static Resolvable GenerateResolvable(object obj)
        {
            if (obj is JObject jObj)
            {
                var typeStr = (jObj["type"]?.ToString()) ?? throw new ResolvableParseException("Failed to parse insufficiently-defined resolvable. All resolvables must have a type field.");

                if (!Enum.TryParse(typeStr, out MutationType type))
                {
                    throw new ResolvableParseException("Failed to parse: " + obj.ToString());
                }

                return type switch
                {
                    MutationType.NONE => throw new ResolvableParseException("You must specify a MutationType"),
                    // Logical
                    MutationType.EQ => new OpEq(jObj),
                    MutationType.NEQ => new OpNeq(jObj),
                    MutationType.AND => new OpAnd(jObj),
                    MutationType.OR => new OpOr(jObj),
                    MutationType.XOR => new OpXor(jObj),
                    // Inequalities
                    MutationType.GT => new OpGt(jObj),
                    MutationType.GTE => new OpGte(jObj),
                    MutationType.LT => new OpLt(jObj),
                    MutationType.LTE => new OpLte(jObj),
                    // Sets
                    MutationType.IN => new OpIn(jObj),
                    MutationType.NIN => new OpNin(jObj),
                    // Strings
                    MutationType.SUBSTRING => new OpStrSub(jObj),
                    MutationType.CONCAT => new OpStrConcat(jObj),
                    // Arrays
                    MutationType.APPEND => new OpArrayAppend(jObj),
                    MutationType.FILTER => new OpArrayFilter(jObj),
                    MutationType.INSERT => new OpArrayInsert(jObj),
                    MutationType.REPLACE => throw new NotImplementedException(),
                    MutationType.DISTINCT => throw new NotImplementedException(),
                    _ => throw new ResolvableParseException("No handler configured for type: " + type.GetDescription()),
                };
            }
            else if (obj is JToken token)
            {
                var str = token.ToString();
                if (str.StartsWith("$"))
                {
                    return new VariableReference(str[1..]);
                }
                else
                {
                    return new ConstantReference(token.Resolve());
                }
            }

            throw new ResolvableParseException("Failed to parse: " + obj?.ToString());
        }

        public static bool IsTruthy(object? obj, JObject root, Dictionary<string, Variable> variables)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj.Equals(false))
            {
                return false;
            }
            if (obj.Equals(""))
            {
                return false;
            }
            if (obj.Equals(0))
            {
                return false;
            }
            if (obj is Resolvable r)
            {
                return IsTruthy(r.Exec(root, variables), root, variables);
            }
            return true;
        }

        public static bool AreEqual(object? a, object? b)
        {
            if (a == null || b == null)
            {
                return a == null && b == null;
            }

            return a.Equals(b);
        }
    }
}
