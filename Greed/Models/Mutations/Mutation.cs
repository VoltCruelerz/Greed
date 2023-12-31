﻿using Newtonsoft.Json.Linq;
using System;

namespace Greed.Models.Mutations
{
    /// <summary>
    /// This abstract class is designed to handle myriad potential mutations one might make to a JSON object.
    /// </summary>
    public abstract class Mutation : Resolvable
    {
        public MutationType Type { get; set; } = MutationType.NONE;

        public Mutation(JObject obj)
        {
            if (Enum.TryParse(obj["type"]?.ToString(), out MutationType type))
            {
                Type = type;
            }
        }

        public Mutation(MutationType type)
        {
            Type = type;
        }
    }
}
