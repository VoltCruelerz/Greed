using Greed.Models.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greed.Models
{
    public class Mod
    {
        private Metadata? Metadata { get; set; }

        public bool IsActive { get; set; } = false;

        public List<Entity> Entities { get; set; } = new List<Entity>();

        public List<LocalizedText> LocalizedTexts { get; set; } = new List<LocalizedText>();

        public bool IsGreedy => Metadata != null;
        public Metadata Meta => Metadata!;

        public Mod(string path)
        {
            Debug.WriteLine("Loading " + path);
            var contents = Directory.GetFiles(path);
            foreach (var item in contents)
            {
                Debug.WriteLine("- " + item);
            }
            var greedyFile = contents.FirstOrDefault(p => p.EndsWith("greed.json"));
            if (greedyFile == null)
            {
                return;
            }

            Metadata = JsonConvert.DeserializeObject<Metadata>(File.ReadAllText(greedyFile));

            var subpaths = Directory.GetDirectories(path);
            var subdirs = subpaths.Select(p => p[path.Length..]);

            // Handle Entities
            if (subdirs.Contains("entities"))
            {
                var entityPath = path + "/entities";
                var entityPaths = Directory.GetFiles(entityPath);
                Entities = entityPaths.Select(p => Entity.BuildEntity(p)).ToList();
            }

            // Handle Text Localization
            if (subdirs.Contains("localized_text"))
            {

            }
        }
    }
}
