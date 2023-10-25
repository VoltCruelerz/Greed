using Greed.Exceptions;
using Greed.Extensions;
using Greed.Models.Mutations.Operations.Arrays;
using Greed.Models.Mutations.Operations.Functions;
using Greed.Models.Mutations.Operations.Functions.Arithmetic;
using Greed.Models.Mutations.Operations.Functions.Comparison;
using Greed.Models.Mutations.Operations.Functions.Comparison.Inequalities;
using Greed.Models.Mutations.Operations.Functions.Logical;
using Greed.Models.Mutations.Operations.Functions.Sets;
using Greed.Models.Mutations.Operations.Functions.Strings;
using Greed.Models.Mutations.Operations.Functions.Variables;
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
            try
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

                        // Arithmetic
                        MutationType.ADD => new OpAdd(jObj),
                        MutationType.SUB => new OpSub(jObj),
                        MutationType.MUL => new OpMul(jObj),
                        MutationType.DIV => new OpDiv(jObj),
                        MutationType.MOD => new OpMod(jObj),

                        // Logical
                        MutationType.AND => new OpAnd(jObj),
                        MutationType.OR => new OpOr(jObj),
                        MutationType.XOR => new OpXor(jObj),
                        MutationType.NOT => new OpNot(jObj),

                        // Comparison
                        MutationType.EQ => new OpEq(jObj),
                        MutationType.NEQ => new OpNeq(jObj),
                        MutationType.GT => new OpGt(jObj),
                        MutationType.GTE => new OpGte(jObj),
                        MutationType.LT => new OpLt(jObj),
                        MutationType.LTE => new OpLte(jObj),

                        // Parameter Sets
                        MutationType.IN => new OpIn(jObj),
                        MutationType.NIN => new OpNin(jObj),

                        // Strings
                        MutationType.SUBSTRING => new OpStrSub(jObj),
                        MutationType.CONCAT => new OpStrConcat(jObj),

                        // Variables
                        MutationType.SET => new OpSetVar(jObj),
                        MutationType.CLEAR => new OpClearVar(jObj),

                        // Arrays
                        MutationType.APPEND => new OpArrayAppend(jObj),
                        MutationType.FILTER => new OpArrayFilter(jObj),
                        MutationType.INSERT => new OpArrayInsert(jObj),
                        MutationType.REPLACE => new OpArrayReplace(jObj),
                        MutationType.INDEX_OF => new OpArrayIndexOf(jObj),
                        MutationType.DISTINCT => throw new NotImplementedException(),

                        _ => throw new ResolvableParseException("No handler configured for type: " + type.GetDescription()),
                    };
                }
                else
                {
                    var str = "";
                    JToken? token = null;
                    if (obj is JToken token1)
                    {
                        token = token1;
                        str = token.ToString();
                    }
                    else if (obj is string strObj)
                    {
                        str = strObj;
                    }
                    else
                    {
                        throw new ResolvableParseException("Failed to parse " + obj?.ToString());
                    }

                    if (str.StartsWith("$"))
                    {
                        return new VariableReference(str[1..]);
                    }
                    if (str.Contains('('))
                    {
                        return GenerateResolvable(OpFunction.ParseToJObject(str));
                    }
                    return new ConstantReference(token!.Resolve());
                }
                throw new ResolvableParseException("Greed is not sure how to parse this Resolvable configuration. Please double-check your JSON");
            }
            catch (Exception ex)
            {
                throw new ResolvableParseException($"Failed to parse configuration to Resolvable:\n{obj?.ToString()}", ex);
            }
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

        public static double ToNumber(object? value, JObject root, Dictionary<string, Variable> variables)
        {
            if (value == null)
            {
                return 0;
            }
            else if (value is string str)
            {
                return double.Parse(str);
            }
            else if (value is bool b)
            {
                return b ? 1 : 0;
            }
            else if (value is int i)
            {
                return i;
            }
            else if (value is long l)
            {
                return l;
            }
            else if (value is float f)
            {
                return f;
            }
            else if (value is double d)
            {
                return d;
            }
            else if (value is Resolvable r)
            {
                return ToNumber(r.Exec(root, variables), root, variables);
            }

            throw new ResolvableParseException($"No handler for converting type {value.GetType()} to a number.");
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
