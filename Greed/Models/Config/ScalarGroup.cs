using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public void Init()
        {
            Scalars.ForEach(s => s.Init());
        }
    }
}
