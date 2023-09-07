using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greed.Models.Online
{
    public class VersionEntry
    {
        [JsonProperty(PropertyName = "download")]
        public string Download { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "sinsVersion")]
        public Version SinsVersion { get; set; } = new Version("0.0.0");

        [JsonProperty(PropertyName = "greedVersion")]
        public Version GreedVersion { get; set; } = new Version("0.0.0");

        [JsonProperty(PropertyName = "dependencies")]
        public List<Dependency> Dependencies { get; set; } = new();

        [JsonProperty(PropertyName = "conflicts")]
        public List<string> Conflicts{ get; set; } = new();

        [JsonProperty(PropertyName = "date")]
        public string DateAdded { get; set; } = DateTime.Today.ToString();
    }
}
