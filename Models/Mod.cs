using Greed.Models.JsonSource;
using Greed.Models.JsonSource.Entities;
using Greed.Models.JsonSource.Text;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
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

        private List<Source> Entities { get; set; } = new List<Source>();

        private List<Source> LocalizedTexts { get; set; } = new List<Source>();

        public bool IsActive { get; set; } = false;

        public string Id { get; set; } = string.Empty;

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
            Id = pathTerms[^1];
            if (enabledModIds.Contains(Id))
            {
                IsActive = true;
            }

            Metadata = JsonConvert.DeserializeObject<Metadata>(File.ReadAllText(greedyFile));

            var subpaths = Directory.GetDirectories(path);
            var subdirs = subpaths.Select(p => p[(path.Length + 1)..]);

            // Handle Entities
            if (subdirs.Contains("entities"))
            {
                var entityPath = path + "\\entities";
                var entityPaths = Directory.GetFiles(entityPath);
                Entities = entityPaths.Select(p => Entity.BuildEntity(p)).ToList();
            }

            // Handle Text Localization
            if (subdirs.Contains("localized_text"))
            {
                // TODO
            }
        }

        public void Export()
        {
            Debug.WriteLine("- Exporting " + Id);
            /*
             * For each source
             *     If gold is required
             *         If gold is already copied
             *             Copy gold -> greed (to provide a baseline)
             *         Append to what's now there.
             *     If gold is not required
             *         Copy src -> greed
             */

            foreach (var entity in Entities)
            {
                Debug.WriteLine("- - " + entity.Filename);
                CreateGreedDirIfNotExists(entity.GreedPath);
                if (entity.NeedsGold)
                {
                    if (!File.Exists(entity.GreedPath))
                    {
                        File.Copy(entity.GoldPath, entity.GreedPath);
                    }
                    File.ReadAllText(entity.GreedPath);

                    var manifest = (EntityManifest)entity;
                    var greedSource = new EntityManifest(entity.GreedPath);
                    greedSource.Ids.ForEach(id => manifest.Ids.Add(id));

                    File.WriteAllText(entity.GreedPath, JsonConvert.SerializeObject(greedSource));
                }
                else
                {
                    File.Copy(entity.SourcePath, entity.GreedPath, false);
                }
            }
        }

        private void CreateGreedDirIfNotExists(string greedPath)
        {
            var info = new FileInfo(greedPath);
            if (!info.Exists)
            {
                Directory.CreateDirectory(info.Directory!.FullName);
            }
        }
    }
}
