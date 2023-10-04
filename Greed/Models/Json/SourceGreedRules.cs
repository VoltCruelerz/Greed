using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Greed.Models.Mutations;

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
        /// The list of mutations
        /// </summary>
        [JsonProperty(PropertyName = "mutations")]
        public List<Mutation>? Mutations { get; set; } = new List<Mutation>();
    }
}
