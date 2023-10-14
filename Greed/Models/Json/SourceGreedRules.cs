using Greed.Models.Mutations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Greed.Models.Json
{
    public class SourceGreedRules
    {
        /// <summary>
        /// If provided, this file should treat the parent as its gold source.
        /// </summary>
        [JsonProperty(PropertyName = "parent")]
        public string? Parent { get; set; }

        /// <summary>
        /// If provided, this is the actual mergename to use.
        /// </summary>
        [JsonProperty(PropertyName = "alias")]
        public string? Alias { get; set; }

        /// <summary>
        /// If provided, this doubles as the *.gmr/u/c, and actually overrides that.
        /// </summary>
        [JsonProperty(PropertyName = "mode")]
        public string? MergeMode { get; set; }

        /// <summary>
        /// Only export this file if the mods in this list are active.
        /// </summary>
        [JsonProperty(PropertyName = "prerequisites")]
        public List<string>? Prerequisites { get; set; } = new List<string>();

        /// <summary>
        /// If provided, this is the order to be exported in. 0 is first infinity is last.
        /// </summary>
        [JsonProperty(PropertyName = "exportOrder")]
        public int ExportOrder { get; set; } = 0;

        /// <summary>
        /// The list of mutations as raw JObjects
        /// </summary>
        [JsonProperty(PropertyName = "mutations")]
        public List<JObject> RawMutations { get; set; } = new();

        /// <summary>
        /// The list of mutations
        /// </summary>
        public List<Mutation> Mutations { get; set; } = new();

        /// <summary>
        /// Deserialization isn't smart enough to handle recursive things like this, apparently. Manual load of raw JObjects.
        /// </summary>
        /// <returns></returns>
        public List<Mutation> InitializeMutations()
        {
            Mutations = RawMutations.Select(m => (Mutation)Resolvable.GenerateResolvable(m)).ToList();
            return Mutations;
        }
    }
}
