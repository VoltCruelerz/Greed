using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows;

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
        public Version Version { get; set; } = new System.Version("0.0.0");

        [JsonRequired]
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; } = string.Empty;

        [JsonRequired]
        [JsonProperty(PropertyName = "greedVersion")]
        public Version GreedVersion { get; set; } = new System.Version("0.0.0");

        [JsonRequired]
        [JsonProperty(PropertyName = "sinsVersion")]
        public Version SinsVersion { get; set; } = new System.Version("0.0.0.0");

        [JsonRequired]
        [JsonProperty(PropertyName = "priority")]
        public int Priority { get; set; }

        [JsonRequired]
        [JsonProperty(PropertyName = "dependencies")]
        public List<string> Dependencies { get; set; } = new List<string>();

        [JsonProperty(PropertyName = "conflicts")]
        public List<string> Conflicts { get; set; } = new List<string>();

        public bool IsLegalVersion(Version liveSinsVersion)
        {
            var liveVersion = Assembly.GetExecutingAssembly().GetName().Version!;
            if (liveVersion.CompareTo(GreedVersion) < 0)
            {
                Debug.WriteLine("Deprecated greed version.");
                return false;
            }
            if (liveSinsVersion.CompareTo(SinsVersion) < 0)
            {
                Debug.WriteLine("Deprecated sins version.");
                return false;
            }
            return true;
        }
    }
}
