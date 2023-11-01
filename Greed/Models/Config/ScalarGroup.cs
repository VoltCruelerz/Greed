using Newtonsoft.Json;
using System.Collections.Generic;

namespace Greed.Models.Config
{
    public class ScalarGroup
    {
        [JsonRequired]
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; } = string.Empty;

        [JsonRequired]
        [JsonProperty(PropertyName = "scalars")]
        public List<GlobalScalar> Scalars { get; set; } = new();

        [JsonProperty(PropertyName = "bools")]
        public List<GlobalBool> Bools { get; set; } = new();

        public void Init()
        {
            Scalars.ForEach(s => s.Init());
        }
    }
}
