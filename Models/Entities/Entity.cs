using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greed.Models.Entities
{
    public class Entity
    {
        public string Id { get; set; }
        public string Json {  get; set; }

        public Entity(string id, string json)
        {
            Id = id;
            Json = json;
        }

        public static Entity BuildEntity(string path)
        {
            var folders = path.Split('\\');
            var mod = folders[path.Length - 2];
            var filename = folders[path.Length - 1];
            var json = File.ReadAllText(path);
            if (filename.EndsWith("entity_manifest"))
            {
                return new EntityManifest(mod, json);
            }
            return new Entity(mod, json);
        }
    }
}
