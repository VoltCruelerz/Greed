using Greed.Models.Mutations.Variables;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

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
    }
}
