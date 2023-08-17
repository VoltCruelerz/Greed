using Greed.Models.JsonSource;
using Greed.Models.JsonSource.Entities;
using Greed.Models.JsonSource.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Greed.Models
{
    public class Mod
    {
        private Metadata? Metadata { get; set; }

        public List<Source> Brushes { get; set; } = new List<Source>();
        public List<Source> Colors { get; set; } = new List<Source>();
        public List<Source> Cursors { get; set; } = new List<Source>();
        public List<Source> DeathSequences { get; set; } = new List<Source>();
        public List<Source> Effects { get; set; } = new List<Source>();
        public List<Source> Fonts { get; set; } = new List<Source>();
        public List<Source> GravityWellProps { get; set; } = new List<Source>();
        public List<Source> Gui { get; set; } = new List<Source>();
        public List<Source> MeshMaterials { get; set; } = new List<Source>();
        public List<Source> Meshes { get; set; } = new List<Source>();
        public List<Source> PlayerColors { get; set; } = new List<Source>();
        public List<Source> PlayerIcons { get; set; } = new List<Source>();
        public List<Source> PlayerPortraits { get; set; } = new List<Source>();
        public List<Source> Scenarios { get; set; } = new List<Source>();
        public List<Source> Shaders { get; set; } = new List<Source>();
        public List<Source> Skyboxes { get; set; } = new List<Source>();
        public List<Source> Sounds { get; set; } = new List<Source>();
        public List<Source> TextureAnimations { get; set; } = new List<Source>();
        public List<Source> Textures { get; set; } = new List<Source>();
        public List<Source> Uniforms { get; set; } = new List<Source>();
        public List<Source> Entities { get; set; } = new List<Source>();
        public List<Source> LocalizedTexts { get; set; } = new List<Source>();

        public bool IsActive { get; set; } = false;
        public string Readme { get; set; } = string.Empty;

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
            var greedMetaFilename = contents.FirstOrDefault(p => p.EndsWith("greed.json"));
            if (greedMetaFilename == null)
            {
                return;
            }

            // Attempt to load the README.md
            var options = new string[] { "readme.md", "readme.txt", "ReadMe.md", "ReadMe.txt", "README.md", "README.txt" };
            var readmeFilename = options.FirstOrDefault(p => File.Exists(path + "\\" + p));
            if (readmeFilename != null)
            {
                Readme = File.ReadAllText(path + "\\" + readmeFilename);
            }

            var pathTerms = path.Split("\\");
            Id = pathTerms[^1];
            if (enabledModIds.Contains(Id))
            {
                IsActive = true;
            }

            Metadata = JsonConvert.DeserializeObject<Metadata>(File.ReadAllText(greedMetaFilename));

            var subpaths = Directory.GetDirectories(path);
            var subdirs = subpaths.Select(p => p[(path.Length + 1)..]);

            Brushes =           ImportFolder(subdirs, path, "brushes", (string p) => new Source(p))                 ?? new List<Source>();
            Colors =            ImportFolder(subdirs, path, "colors", (string p) => new Source(p))                  ?? new List<Source>();
            Cursors =           ImportFolder(subdirs, path, "cursors", (string p) => new Source(p))                 ?? new List<Source>();
            DeathSequences =    ImportFolder(subdirs, path, "death_sequences", (string p) => new Source(p))         ?? new List<Source>();
            Effects =           ImportFolder(subdirs, path, "effects", (string p) => new Source(p))                 ?? new List<Source>();
            Entities =          ImportFolder(subdirs, path, "entities", Entity.Create)                              ?? new List<Source>();
            Fonts =             ImportFolder(subdirs, path, "fonts", (string p) => new Source(p))                   ?? new List<Source>();
            GravityWellProps =  ImportFolder(subdirs, path, "gravity_well_props", (string p) => new Source(p))      ?? new List<Source>();
            Gui =               ImportFolder(subdirs, path, "gui", (string p) => new Source(p))                     ?? new List<Source>();
            MeshMaterials =     ImportFolder(subdirs, path, "mesh_materials", (string p) => new Source(p))          ?? new List<Source>();
            Meshes =            ImportFolder(subdirs, path, "meshes", (string p) => new Source(p))                  ?? new List<Source>();
            PlayerColors =      ImportFolder(subdirs, path, "player_colors", (string p) => new Source(p))           ?? new List<Source>();
            PlayerIcons =       ImportFolder(subdirs, path, "player_icons", (string p) => new Source(p))            ?? new List<Source>();
            PlayerPortraits =   ImportFolder(subdirs, path, "player_portraits", (string p) => new Source(p))        ?? new List<Source>();
            Scenarios =         ImportFolder(subdirs, path, "scenarios", (string p) => new Source(p))               ?? new List<Source>();
            Shaders =           ImportFolder(subdirs, path, "shaders", (string p) => new Source(p))                 ?? new List<Source>();
            Skyboxes =          ImportFolder(subdirs, path, "skyboxes", (string p) => new Source(p))                ?? new List<Source>();
            Sounds =            ImportFolder(subdirs, path, "sounds", (string p) => new Source(p))                  ?? new List<Source>();
            TextureAnimations = ImportFolder(subdirs, path, "texture_animations", (string p) => new Source(p))      ?? new List<Source>();
            Textures =          ImportFolder(subdirs, path, "textures", (string p) => new Source(p))                ?? new List<Source>();
            Uniforms =          ImportFolder(subdirs, path, "uniforms", (string p) => new Source(p))                ?? new List<Source>();
            LocalizedTexts =    ImportFolder(subdirs, path, "localized_text", (string p) => new LocalizedText(p))   ?? new List<Source>();
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

            ExportFolder(Brushes,               (greedPath, modSource) => new Source(greedPath).Merge(modSource));
            ExportFolder(Colors,                (greedPath, modSource) => new Source(greedPath).Merge(modSource));
            ExportFolder(Cursors,               (greedPath, modSource) => new Source(greedPath).Merge(modSource));
            ExportFolder(DeathSequences,        (greedPath, modSource) => new Source(greedPath).Merge(modSource));
            ExportFolder(Effects,               (greedPath, modSource) => new Source(greedPath).Merge(modSource));
            ExportFolder(Entities,              (greedPath, modSource) => Entity.Create(greedPath).Merge(modSource));
            ExportFolder(Fonts,                 (greedPath, modSource) => new Source(greedPath).Merge(modSource));
            ExportFolder(GravityWellProps,      (greedPath, modSource) => new Source(greedPath).Merge(modSource));
            ExportFolder(Gui,                   (greedPath, modSource) => new Source(greedPath).Merge(modSource));
            ExportFolder(MeshMaterials,         (greedPath, modSource) => new Source(greedPath).Merge(modSource));
            ExportFolder(Meshes,                (greedPath, modSource) => new Source(greedPath).Merge(modSource));
            ExportFolder(PlayerColors,          (greedPath, modSource) => new Source(greedPath).Merge(modSource));
            ExportFolder(PlayerIcons,           (greedPath, modSource) => new Source(greedPath).Merge(modSource));
            ExportFolder(PlayerPortraits,       (greedPath, modSource) => new Source(greedPath).Merge(modSource));
            ExportFolder(Scenarios,             (greedPath, modSource) => new Source(greedPath).Merge(modSource));
            ExportFolder(Shaders,               (greedPath, modSource) => new Source(greedPath).Merge(modSource));
            ExportFolder(Skyboxes,              (greedPath, modSource) => new Source(greedPath).Merge(modSource));
            ExportFolder(Sounds,                (greedPath, modSource) => new Source(greedPath).Merge(modSource));
            ExportFolder(TextureAnimations,     (greedPath, modSource) => new Source(greedPath).Merge(modSource));
            ExportFolder(Textures,              (greedPath, modSource) => new Source(greedPath).Merge(modSource));
            ExportFolder(Uniforms,              (greedPath, modSource) => new Source(greedPath).Merge(modSource));
            ExportFolder(LocalizedTexts,        (greedPath, modSource) => new LocalizedText(greedPath).Merge((LocalizedText)modSource));
        }

        private static void ExportFolder(List<Source> sources, Func<string, Source, Source> mergeHandler)
        {
            foreach (var source in sources)
            {
                Debug.WriteLine("- - " + source.Filename);
                CreateGreedDirIfNotExists(source.GreedPath);

                var greedExists = File.Exists(source.GreedPath);
                var goldExists = File.Exists(source.GoldPath);

                // Not all types are initialized from gold.
                var initializeFromGold = source is not EntityManifest;

                // If greed doesn't exist yet, initialize it from gold
                if (!greedExists && goldExists && initializeFromGold)
                {
                    File.Copy(source.GoldPath, source.GreedPath);
                    greedExists = true;
                }

                // If greed exists, we need to merge the source into it.
                var output = greedExists
                    ? mergeHandler(source.GreedPath, source)
                    : source;

                // Write to greed path.
                File.WriteAllText(source.GreedPath, JsonConvert.SerializeObject(JObject.Parse(output.Json), Formatting.Indented));
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

        public string DiffFromGold()
        {
            var sb = new StringBuilder();
            sb.AppendLine(Id);
            Entities.ForEach(p => sb.AppendLine("Entity " + p.DiffFromGold()));
            LocalizedTexts.ForEach(p =>  sb.AppendLine("Localized Text " + p.DiffFromGold()));

            return sb.ToString();
        }
    }
}
