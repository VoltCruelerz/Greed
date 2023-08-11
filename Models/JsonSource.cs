using Greed.Models.Entities;
using System.IO;

namespace Greed.Models
{
    public class JsonSource
    {
        public string Mod { get; set; }
        public string Name { get; set; }
        public string Json { get; set; }

        public JsonSource(string path)
        {
            var folders = path.Split('\\');
            Mod = folders[path.Length - 2];
            Name = folders[path.Length - 1];
            Json = File.ReadAllText(path);
        }

        public static JsonSource BuildEntity(string path)
        {
            var folders = path.Split('\\');
            var mod = folders[path.Length - 2];
            var filename = folders[path.Length - 1];
            var json = File.ReadAllText(path);
            if (filename.EndsWith("entity_manifest"))
            {
                return new EntityManifest(path);
            }
            return new Entity(path);
        }
    }
}
