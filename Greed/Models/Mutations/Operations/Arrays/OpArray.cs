using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greed.Models.Mutations.Operations.Arrays
{
    public abstract class OpArray : Mutation
    {
        public string[] Path { get; set; }

        public OpArray(JObject obj) : base(obj)
        {
            Path = obj["path"]!.ToString().Split(".");
        }
    }
}
