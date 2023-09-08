using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Greed.Models.Online
{
    public class VersionEntry
    {
        [JsonProperty(PropertyName = "download")]
        public string Download { get; set; } = string.Empty;

        [JsonRequired]
        [JsonProperty(PropertyName = "sinsVersion")]
        public Version SinsVersion { get; set; } = new Version("0.0.0");

        [JsonRequired]
        [JsonProperty(PropertyName = "greedVersion")]
        public Version GreedVersion { get; set; } = new Version("0.0.0");

        [JsonRequired]
        [JsonProperty(PropertyName = "dependencies")]
        public List<Dependency> Dependencies { get; set; } = new();

        [JsonRequired]
        [JsonProperty(PropertyName = "conflicts")]
        public List<string> Conflicts { get; set; } = new();

        [JsonRequired]
        [JsonProperty(PropertyName = "date")]
        public string DateAdded { get; set; } = DateTime.Today.ToString();
    }
}
