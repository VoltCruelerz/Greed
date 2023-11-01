using Newtonsoft.Json;

namespace Greed.Models.Config
{
    public class DirConfig
    {
        [JsonRequired]
        [JsonProperty(PropertyName = "mods")]
        public string Mods { get; set; } = string.Empty;

        [JsonRequired]
        [JsonProperty(PropertyName = "export")]
        public string Export { get; set; } = string.Empty;

        [JsonRequired]
        [JsonProperty(PropertyName = "sins")]
        public string Sins { get; set; } = string.Empty;

        [JsonRequired]
        [JsonProperty(PropertyName = "download")]
        public string Download { get; set; } = string.Empty;
    }
}
