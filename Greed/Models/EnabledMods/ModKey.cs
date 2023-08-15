﻿using Newtonsoft.Json;

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
