using Greed.Models.JsonSource;
using Greed.Models.JsonSource.Entities;
using Greed.Models.JsonSource.Text;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;


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

            Entities = ImportFolder(subdirs, path, "entities", (string filePath) => Entity.BuildEntity(filePath))
                ?? LocalizedTexts;
            LocalizedTexts = ImportFolder(subdirs, path, "localized_text", (string filePath) => new LocalizedText(filePath))
                ?? LocalizedTexts;
        }

        private static List<Source>? ImportFolder(IEnumerable<string> subdirs, string path, string folder, Func<string, Source> handleFileImport)
        {
            if (subdirs.Contains(folder))
            {
                return Directory.GetFiles(path + "\\" + folder)
                    .Select(p => handleFileImport(p))
                    .ToList();
            }
            return null;
        }

        public void Export()
        {
            Debug.WriteLine("- Exporting " + Id);

            ExportFolder(Entities, (Source source) =>
            {
                var manifest = (EntityManifest)source;
                var greedSource = new EntityManifest(source.GreedPath);
                manifest.Ids.ForEach(id => greedSource.Upsert(id));
                return greedSource;
            });

            ExportFolder(LocalizedTexts, (Source source) =>
            {
                var local = (LocalizedText)source;
                var greedSource = new LocalizedText(source.GreedPath);
                local.Text.ForEach(kv => greedSource.Upsert(kv));
                return greedSource;
            });
        }

        private void ExportFolder(List<Source> sources, Func<Source, Source> handleFileExport)
        {
            /*
             * For each source
             *     If gold is required
             *         If gold is already copied
             *             Copy gold -> greed (to provide a baseline)
             *         Append to what's now there.
             *     If gold is not required
             *         Copy src -> greed
             */
            foreach (var source in sources)
            {
                Debug.WriteLine("- - " + source.Filename);
                CreateGreedDirIfNotExists(source.GreedPath);
                if (source.NeedsMerge)
                {
                    var greedExists = File.Exists(source.GreedPath);
                    var output = source;

                    // If we need to initialize with a gold copy.
                    if (source.NeedsGold)
                    {
                        // If greed doesn't exist yet, initialize it from gold.
                        if (!greedExists)
                        {
                            File.Copy(source.GoldPath, source.GreedPath);
                        }
                        // We need to merge the source with what's out there.
                        output = handleFileExport(source);
                    }
                    else if (greedExists)
                    {
                        // If we don't need to initialize with gold and greed already exists
                        output = handleFileExport(source);
                    }
                    else
                    {
                        // If we don't need to initialize with gold and greed doesn't exist yet, start from scratch.
                    }


                    File.WriteAllText(source.GreedPath, JsonConvert.SerializeObject(output));
                }
                else
                {
                    File.Copy(source.SourcePath, source.GreedPath, false);
                }
            }
        }

        private static void CreateGreedDirIfNotExists(string greedPath)
        {
            var info = new FileInfo(greedPath);
            if (!info.Exists)
            {
                Directory.CreateDirectory(info.Directory!.FullName);
            }
        }
    }
}
