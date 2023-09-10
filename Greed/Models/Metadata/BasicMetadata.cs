using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Greed.Models.Metadata
{
    public abstract class BasicMetadata
    {

        [JsonRequired]
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; } = string.Empty;

        [JsonRequired]
        [JsonProperty(PropertyName = "author")]
        public string Author { get; set; } = string.Empty;

        [JsonRequired]
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; } = string.Empty;

        [JsonRequired]
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "isTotalConversion")]
        public bool IsTotalConversion { get; set; } = false;

        public abstract Version GetVersion();

        public abstract Version GetGreedVersion();

        public abstract Version GetSinsVersion();

        public abstract List<Dependency> GetDependencies();
        public abstract List<string> GetConflicts();
    }
}
