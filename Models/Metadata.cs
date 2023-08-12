using Newtonsoft.Json;
using System.Collections.Generic;

namespace Greed.Models
{
    public class Metadata
    {
        [JsonRequired]
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; } = string.Empty;

        [JsonRequired]
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; } = string.Empty;

        [JsonRequired]
        [JsonProperty(PropertyName = "author")]
        public string Author { get; set; } = string.Empty;

        [JsonRequired]
        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; } = string.Empty;

        [JsonRequired]
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; } = string.Empty;

        [JsonRequired]
        [JsonProperty(PropertyName = "sinsVersion")]
        public string SinsVersion { get; set; } = string.Empty;

        [JsonRequired]
        [JsonProperty(PropertyName = "priority")]
        public int Priority { get; set; }

        [JsonRequired]
        [JsonProperty(PropertyName = "dependencies")]
        public List<string> Dependencies { get; set; } = new List<string>();

        [JsonProperty(PropertyName = "conflicts")]
        public List<string> Conflicts { get; set; } = new List<string>();
    }
}
