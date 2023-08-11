using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greed.Models.Entities
{
    public class EntityManifest : Entity
    {
        public List<string> ids;

        public EntityManifest(string id, string json) : base(id, json)
        {
            var manifest = JObject.Parse(json);
            var arr = (JArray)manifest["ids"];
            ids = arr.Select(i => i.ToString()).ToList();
        }
    }
}
