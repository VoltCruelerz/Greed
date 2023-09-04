using Newtonsoft.Json;

namespace Greed.Models.Metadata
{
    public class OnlineMetadata : LocalMetadata
    {
        //[JsonRequired]// TODO - reactivate once list is updated
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; } = string.Empty;

        //[JsonRequired]// TODO - reactivate once list is updated
        [JsonProperty(PropertyName = "lastUpdated")]
        public string LastUpdated { get; set; } = string.Empty;
    }
}
