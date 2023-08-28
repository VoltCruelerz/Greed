using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Greed.Models.Json.Entities
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class EntityManifest : Entity
    {
        [JsonProperty(PropertyName = "ids")]
        public readonly List<string> Ids;

        public EntityManifest(string path) : base(path)
        {
            var manifest = JObject.Parse(Json);
            var arr = (JArray)manifest["ids"]!;
            Ids = arr.Select(i => i.ToString()).ToList();
        }

        /// <summary>
        /// If you're making multiple changes, use UpsertRange() instead.
        /// </summary>
        /// <param name="key"></param>
        public void Upsert(string key)
        {
            if (!Ids.Contains(key))
            {
                Ids.Add(key);
            }
            Json = JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        /// <summary>
        /// Thisis more efficient than the other one.
        /// </summary>
        /// <param name="keys"></param>
        public void UpsertRange(List<string> keys)
        {
            foreach (var key in keys)
            {
                if (!Ids.Contains(key))
                {
                    Ids.Add(key);
                }
            }
            Json = JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public override JsonSource Merge(JsonSource other)
        {
            var otherManifest = other as EntityManifest ?? throw new InvalidOperationException($"Attempted to merge entity {other.SourcePath} into manifest {this.SourcePath}");
            UpsertRange(otherManifest.Ids);
            return this;
        }

        public override JsonSource Clone()
        {
            return new EntityManifest(SourcePath);
        }
    }
}
