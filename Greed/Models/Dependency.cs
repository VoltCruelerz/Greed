using Newtonsoft.Json;
using System;

namespace Greed.Models
{
    public class Dependency
    {

        [JsonRequired]
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; } = string.Empty;

        [JsonRequired]
        [JsonProperty(PropertyName = "version")]
        public Version Version { get; set; } = new Version("0.0.0");

        public override string ToString()
        {
            return Id + " v" + Version.ToString();
        }
    }
}
