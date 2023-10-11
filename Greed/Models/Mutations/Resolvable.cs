using Greed.Exceptions;
using Greed.Extensions;
using Greed.Models.Mutations.Operations.Arrays;
using Greed.Models.Mutations.Operations.Logical;
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
            if (obj is JObject jobj)
            {
                var typeStr = (jobj["type"]?.ToString()) ?? throw new ResolvableParseException("Failed to parse insufficiently-defined resolvable. All resolvables must have a type field.");

                if (!Enum.TryParse(typeStr, out MutationType type))
                {
                    throw new ResolvableParseException("Failed to parse: " + obj.ToString());
                }

                return type switch
                {
                    MutationType.NONE => throw new NotImplementedException(),
                    MutationType.EQ => new OpEq(jobj),
                    MutationType.NEQ => new OpNeq(jobj),
                    MutationType.CONCAT => new OpConcat(jobj),
                    MutationType.INSERT => throw new NotImplementedException(),
                    MutationType.FILTER => throw new NotImplementedException(),
                    MutationType.REPLACE => throw new NotImplementedException(),
                    _ => throw new ResolvableParseException("Unrecognized type " + type.GetDescription()),
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
                    return token.Type switch
                    {
                        JTokenType.Integer => new ConstantReference(token.Value<int>()),
                        JTokenType.Float => new ConstantReference(token.Value<float>()),
                        JTokenType.Boolean => new ConstantReference(token.Value<bool>()),
                        JTokenType.String => new ConstantReference(token.Value<string>()),
                        _ => throw new ResolvableParseException("Unrecognized type " + token.Type),
                    };
                }
            }

            throw new ResolvableParseException("Failed to parse: " + obj?.ToString());
        }

        public static bool IsTruthy(object? obj)
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
            return true;
        }
    }
}
