using Newtonsoft.Json;
using System.Collections.Generic;

namespace Greed.Models.EnabledMods
{
    public class EnabledMods
    {
        [JsonRequired]
        [JsonProperty(PropertyName = "mod_keys")]
        public List<ModKey> ModKeys { get; set; } = new();
    }
}
