using Greed.Models.Mutations.Paths;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Greed.Models.Mutations.Operations.Arrays
{
    public abstract class OpArray : Mutation
    {
        public string PathStr { get; set; }
        public List<ActionPath> Path { get; set; }

        public OpArray(JObject obj) : base(obj)
        {
            PathStr = obj["path"]!.ToString();
            Path = ActionPath.Build(PathStr);
        }
    }
}
