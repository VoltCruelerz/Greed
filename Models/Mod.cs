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

        public string Id { get; set; } = string.Empty;

        public List<JsonSource> Entities { get; set; } = new List<JsonSource>();

        public List<LocalizedText> LocalizedTexts { get; set; } = new List<LocalizedText>();

        public bool IsGreedy => Metadata != null;
        public Metadata Meta => Metadata!;

        public Mod(List<string> enabledModIds, string path)
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

            var pathTerms = path.Split("\\");
            Id = pathTerms[pathTerms.Length - 1];
            if (enabledModIds.Contains(Id))
            {
                IsActive = true;
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
