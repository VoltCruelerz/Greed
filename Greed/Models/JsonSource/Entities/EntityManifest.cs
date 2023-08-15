using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Greed.Models.JsonSource.Entities
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class EntityManifest : Source
    {
        [JsonProperty(PropertyName = "ids")]
        public readonly List<string> Ids;

        public EntityManifest(string path) : base(path)
        {
            NeedsMerge = true;
            var manifest = JObject.Parse(Json);
            var arr = (JArray)manifest["ids"]!;
            Ids = arr.Select(i => i.ToString()).ToList();
        }

        public void Upsert(string key)
        {
            if (!Ids.Contains(key))
            {
                Ids.Add(key);
            }
        }
    }
}
