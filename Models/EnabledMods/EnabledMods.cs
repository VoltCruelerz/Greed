using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Greed.Models.EnabledMods
{
    public class EnabledMods
    {
        [JsonRequired]
        [JsonProperty(PropertyName = "mod_keys")]
        public List<ModKey> ModKeys { get; set; } = new();
    }
}
