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

        private List<Source> Brushes { get; set; } = new List<Source>();
        private List<Source> Colors { get; set; } = new List<Source>();
        private List<Source> Cursors { get; set; } = new List<Source>();
        private List<Source> DeathSequences { get; set; } = new List<Source>();
        private List<Source> Effects { get; set; } = new List<Source>();
        private List<Source> Fonts { get; set; } = new List<Source>();
        private List<Source> GravityWellProps { get; set; } = new List<Source>();
        private List<Source> Gui { get; set; } = new List<Source>();
        private List<Source> MeshMaterials { get; set; } = new List<Source>();
        private List<Source> Meshes { get; set; } = new List<Source>();
        private List<Source> PlayerColors { get; set; } = new List<Source>();
        private List<Source> PlayerIcons { get; set; } = new List<Source>();
        private List<Source> PlayerPortraits { get; set; } = new List<Source>();
        private List<Source> Scenarios { get; set; } = new List<Source>();
        private List<Source> Shaders { get; set; } = new List<Source>();
        private List<Source> Skyboxes { get; set; } = new List<Source>();
        private List<Source> Sounds { get; set; } = new List<Source>();
        private List<Source> TextureAnimations { get; set; } = new List<Source>();
        private List<Source> Textures { get; set; } = new List<Source>();
        private List<Source> Uniforms { get; set; } = new List<Source>();
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

            // Not yet validated.
            Brushes = ImportFolder(subdirs, path, "brushes", (string filePath) => new Source(filePath))
                ?? Brushes;
            Colors = ImportFolder(subdirs, path, "colors", (string filePath) => new Source(filePath))
                ?? Colors;
            Cursors = ImportFolder(subdirs, path, "cursors", (string filePath) => new Source(filePath))
                ?? Cursors;
            DeathSequences = ImportFolder(subdirs, path, "death_sequences", (string filePath) => new Source(filePath))
                ?? DeathSequences;
            Effects = ImportFolder(subdirs, path, "effects", (string filePath) => new Source(filePath))
                ?? Effects;
            Fonts = ImportFolder(subdirs, path, "fonts", (string filePath) => new Source(filePath))
                ?? Fonts;
            GravityWellProps = ImportFolder(subdirs, path, "gravity_well_props", (string filePath) => new Source(filePath))
                ?? GravityWellProps;
            Gui = ImportFolder(subdirs, path, "gui", (string filePath) => new Source(filePath))
                ?? Gui;
            MeshMaterials = ImportFolder(subdirs, path, "mesh_materials", (string filePath) => new Source(filePath))
                ?? MeshMaterials;
            Meshes = ImportFolder(subdirs, path, "meshes", (string filePath) => new Source(filePath))
                ?? Meshes;
            PlayerColors = ImportFolder(subdirs, path, "player_colors", (string filePath) => new Source(filePath))
                ?? PlayerColors;
            PlayerIcons = ImportFolder(subdirs, path, "player_icons", (string filePath) => new Source(filePath))
                ?? PlayerIcons;
            PlayerPortraits = ImportFolder(subdirs, path, "player_portraits", (string filePath) => new Source(filePath))
                ?? PlayerPortraits;
            Scenarios = ImportFolder(subdirs, path, "scenarios", (string filePath) => new Source(filePath))
                ?? Scenarios;
            Shaders = ImportFolder(subdirs, path, "shaders", (string filePath) => new Source(filePath))
                ?? Shaders;
            Skyboxes = ImportFolder(subdirs, path, "skyboxes", (string filePath) => new Source(filePath))
                ?? Skyboxes;
            Sounds = ImportFolder(subdirs, path, "sounds", (string filePath) => new Source(filePath))
                ?? Sounds;
            TextureAnimations = ImportFolder(subdirs, path, "texture_animations", (string filePath) => new Source(filePath))
                ?? TextureAnimations;
            Textures = ImportFolder(subdirs, path, "textures", (string filePath) => new Source(filePath))
                ?? Textures;
            Uniforms = ImportFolder(subdirs, path, "uniforms", (string filePath) => new Source(filePath))
                ?? Uniforms;

            // Validated
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

            // Not validated
            ExportFolder(Brushes, (Source source) => source);
            ExportFolder(Colors, (Source source) => source);
            ExportFolder(Cursors, (Source source) => source);
            ExportFolder(DeathSequences, (Source source) => source);
            ExportFolder(Effects, (Source source) => source);
            ExportFolder(Fonts, (Source source) => source);
            ExportFolder(GravityWellProps, (Source source) => source);
            ExportFolder(Gui, (Source source) => source);
            ExportFolder(MeshMaterials, (Source source) => source);
            ExportFolder(Meshes, (Source source) => source);
            ExportFolder(PlayerColors, (Source source) => source);
            ExportFolder(PlayerIcons, (Source source) => source);
            ExportFolder(PlayerPortraits, (Source source) => source);
            ExportFolder(Scenarios, (Source source) => source);
            ExportFolder(Shaders, (Source source) => source);
            ExportFolder(Skyboxes, (Source source) => source);
            ExportFolder(Sounds, (Source source) => source);
            ExportFolder(TextureAnimations, (Source source) => source);
            ExportFolder(Textures, (Source source) => source);
            ExportFolder(Uniforms, (Source source) => source);

            // Validated
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

        private static void ExportFolder(List<Source> sources, Func<Source, Source> handleFileExport)
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
