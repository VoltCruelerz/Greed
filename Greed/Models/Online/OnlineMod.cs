﻿using Greed.Models.Metadata;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greed.Models.Online
{
    public class OnlineMod : BasicMetadata
    {
        [JsonRequired]
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; } = string.Empty;

        [JsonRequired]
        [JsonProperty(PropertyName = "versions")]
        public Dictionary<string, VersionEntry> Versions { get; set; } = new();

        [JsonRequired]
        [JsonProperty(PropertyName = "latest")]
        public Version Latest { get; set; } = new Version("0.0.0");

        public VersionEntry Live => Versions[Latest.ToString()];

        public override Version GetVersion()
        {
            return Latest;
        }

        public override Version GetGreedVersion()
        {
            return Live.GreedVersion;
        }

        public override Version GetSinsVersion()
        {
            return Live.SinsVersion;
        }

        public override List<Dependency> GetDependencies() { return Live.Dependencies; }

        public override List<string> GetConflicts() { return Live.Conflicts; }
    }
}
