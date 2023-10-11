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
    public abstract class Mutation : Resolvable
    {
        public MutationType Type { get; set; } = MutationType.NONE;

        public Dictionary<string, Variable> Variables = new();

        public Mutation(JObject obj)
        {
            if (Enum.TryParse(obj["type"]?.ToString(), out MutationType type))
            {
                Type = type;
            }
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
