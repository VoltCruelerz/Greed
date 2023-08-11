using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Greed.Models.EnabledMods
{
    public class ModKey
    {
        [JsonRequired]
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; } = string.Empty;

        [JsonRequired]
        [JsonProperty(PropertyName = "version")]
        public int Version { get; set; }
    }
}
