using Greed.Models.Json;
using Greed.Models.Json.Entities;
using Greed.Models.Json.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using FontFamily = System.Windows.Media.FontFamily;

namespace Greed.Models
{
    public partial class Mod
    {
        private Metadata? Metadata { get; set; }

        // Json
        public List<JsonSource> Brushes { get; set; } = new List<JsonSource>();
        public List<JsonSource> Colors { get; set; } = new List<JsonSource>();
        public List<JsonSource> Cursors { get; set; } = new List<JsonSource>();
        public List<JsonSource> DeathSequences { get; set; } = new List<JsonSource>();
        public List<JsonSource> Effects { get; set; } = new List<JsonSource>();
        public List<JsonSource> Fonts { get; set; } = new List<JsonSource>();
        public List<JsonSource> GravityWellProps { get; set; } = new List<JsonSource>();
        public List<JsonSource> Gui { get; set; } = new List<JsonSource>();
        public List<JsonSource> MeshMaterials { get; set; } = new List<JsonSource>();
        public List<JsonSource> PlayerColors { get; set; } = new List<JsonSource>();
        public List<JsonSource> PlayerIcons { get; set; } = new List<JsonSource>();
        public List<JsonSource> PlayerPortraits { get; set; } = new List<JsonSource>();
        public List<JsonSource> Skyboxes { get; set; } = new List<JsonSource>();
        public List<JsonSource> TextureAnimations { get; set; } = new List<JsonSource>();
        public List<JsonSource> Uniforms { get; set; } = new List<JsonSource>();
        public List<JsonSource> Entities { get; set; } = new List<JsonSource>();
        public List<JsonSource> LocalizedTexts { get; set; } = new List<JsonSource>();

        // Non-Json
        public List<Source> Meshes { get; set; } = new List<Source>();
        public List<Source> Scenarios { get; set; } = new List<Source>();
        public List<Source> Shaders { get; set; } = new List<Source>();
        public List<Source> Sounds { get; set; } = new List<Source>();
        public List<Source> Textures { get; set; } = new List<Source>();

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

            // Json data
            Brushes = ImportJsonFolder(subdirs, path, "brushes", (p) => new JsonSource(p));
            Colors = ImportJsonFolder(subdirs, path, "colors", (p) => new JsonSource(p));
            Cursors = ImportJsonFolder(subdirs, path, "cursors", (p) => new JsonSource(p));
            DeathSequences = ImportJsonFolder(subdirs, path, "death_sequences", (p) => new JsonSource(p));
            Effects = ImportJsonFolder(subdirs, path, "effects", (p) => new JsonSource(p));
            Entities = ImportJsonFolder(subdirs, path, "entities", Entity.Create);
            Fonts = ImportJsonFolder(subdirs, path, "fonts", (p) => new JsonSource(p));
            GravityWellProps = ImportJsonFolder(subdirs, path, "gravity_well_props", (p) => new JsonSource(p));
            Gui = ImportJsonFolder(subdirs, path, "gui", (p) => new JsonSource(p));
            MeshMaterials = ImportJsonFolder(subdirs, path, "mesh_materials", (p) => new JsonSource(p));
            PlayerColors = ImportJsonFolder(subdirs, path, "player_colors", (p) => new JsonSource(p));
            PlayerIcons = ImportJsonFolder(subdirs, path, "player_icons", (p) => new JsonSource(p));
            PlayerPortraits = ImportJsonFolder(subdirs, path, "player_portraits", (p) => new JsonSource(p));
            Skyboxes = ImportJsonFolder(subdirs, path, "skyboxes", (p) => new JsonSource(p));
            TextureAnimations = ImportJsonFolder(subdirs, path, "texture_animations", (p) => new JsonSource(p));
            Uniforms = ImportJsonFolder(subdirs, path, "uniforms", (p) => new JsonSource(p));
            LocalizedTexts = ImportJsonFolder(subdirs, path, "localized_text", (p) => new LocalizedText(p));

            // Non-JSON
            Sounds = ImportFolder(subdirs, path, "sounds");
            Shaders = ImportFolder(subdirs, path, "shaders");
            Scenarios = ImportFolder(subdirs, path, "scenarios");
            Meshes = ImportFolder(subdirs, path, "meshes");
            Textures = ImportFolder(subdirs, path, "textures");
        }

        private static List<JsonSource> ImportJsonFolder(IEnumerable<string> subdirs, string path, string folder, Func<string, JsonSource> handleFileImport)
        {
            if (subdirs.Contains(folder))
            {
                return Directory.GetFiles(path + "\\" + folder)
                    .Select(p => handleFileImport(p))
                    .ToList();
            }
            return new List<JsonSource>();
        }

        private static List<Source> ImportFolder(IEnumerable<string> subdirs, string path, string folder)
        {
            if (subdirs.Contains(folder))
            {
                return Directory.GetFiles(path + "\\" + folder)
                    .Select(p => new Source(p))
                    .ToList();
            }
            return new List<Source>();
        }

        public void Export()
        {
            Debug.WriteLine("- Exporting " + Id);

            // Merge Json
            ExportJsonFolder(Brushes, (greedPath, modSource) => new JsonSource(greedPath).Merge(modSource));
            ExportJsonFolder(Colors, (greedPath, modSource) => new JsonSource(greedPath).Merge(modSource));
            ExportJsonFolder(Cursors, (greedPath, modSource) => new JsonSource(greedPath).Merge(modSource));
            ExportJsonFolder(DeathSequences, (greedPath, modSource) => new JsonSource(greedPath).Merge(modSource));
            ExportJsonFolder(Effects, (greedPath, modSource) => new JsonSource(greedPath).Merge(modSource));
            ExportJsonFolder(Entities, (greedPath, modSource) => Entity.Create(greedPath).Merge(modSource));
            ExportJsonFolder(Fonts, (greedPath, modSource) => new JsonSource(greedPath).Merge(modSource));
            ExportJsonFolder(GravityWellProps, (greedPath, modSource) => new JsonSource(greedPath).Merge(modSource));
            ExportJsonFolder(Gui, (greedPath, modSource) => new JsonSource(greedPath).Merge(modSource));
            ExportJsonFolder(MeshMaterials, (greedPath, modSource) => new JsonSource(greedPath).Merge(modSource));
            ExportJsonFolder(PlayerColors, (greedPath, modSource) => new JsonSource(greedPath).Merge(modSource));
            ExportJsonFolder(PlayerIcons, (greedPath, modSource) => new JsonSource(greedPath).Merge(modSource));
            ExportJsonFolder(PlayerPortraits, (greedPath, modSource) => new JsonSource(greedPath).Merge(modSource));
            ExportJsonFolder(Skyboxes, (greedPath, modSource) => new JsonSource(greedPath).Merge(modSource));
            ExportJsonFolder(TextureAnimations, (greedPath, modSource) => new JsonSource(greedPath).Merge(modSource));
            ExportJsonFolder(Uniforms, (greedPath, modSource) => new JsonSource(greedPath).Merge(modSource));
            ExportJsonFolder(LocalizedTexts, (greedPath, modSource) => new LocalizedText(greedPath).Merge((LocalizedText)modSource));

            // Overwrite binary types
            ExportSourceFolder(Meshes);
            ExportSourceFolder(Scenarios);
            ExportSourceFolder(Shaders);
            ExportSourceFolder(Sounds);
            ExportSourceFolder(Textures);
        }

        private static void ExportJsonFolder(List<JsonSource> sources, Func<string, JsonSource, JsonSource> mergeHandler)
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

        private static void ExportSourceFolder(List<Source> sources)
        {
            foreach (var source in sources)
            {
                Debug.WriteLine("- - " + source.Filename);
                CreateGreedDirIfNotExists(source.GreedPath);
                File.Copy(source.SourcePath, source.GreedPath);
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
            LocalizedTexts.ForEach(p => sb.AppendLine("Localized Text " + p.DiffFromGold()));

            return sb.ToString();
        }

        public void RenderReadme(BlockCollection blocks)
        {
            var listRegex = GetIsListRegex();
            var noSpace = new Thickness(0);
            var lines = Readme.Split("\r\n")
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();

            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                var trimmed = line.Trim();
                if (line.StartsWith("#"))
                {
                    var boost = 2 * (5 - line.Count(c => c == '#'));
                    var titleText = line.Substring(line.IndexOf(" ") + 1);
                    var p = new Paragraph();
                    FormatLine(p.Inlines, titleText, 10 + boost, true);
                    blocks.Add(p);
                }
                else if (trimmed.StartsWith("*") || trimmed.StartsWith("-"))
                {
                    var p = new Paragraph();
                    FormatLine(p.Inlines, line + "\r\n");
                    for (int j = i + 1; j < lines.Count; j++)
                    {
                        var trimJ = lines[j].Trim();
                        if (GetIsListRegex().IsMatch(trimJ))
                        {
                            FormatLine(p.Inlines, lines[j] + "\r\n");
                            i++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    blocks.Add(p);
                } else
                {
                    var p = new Paragraph();
                    FormatLine(p.Inlines, line);
                    blocks.Add(p);
                }
            };
        }

        private void FormatLine(InlineCollection inlines, string line, int fontSize = 10, bool forceBold = false, bool forceItalics = false)
        {
            var consolas = new FontFamily("Consolas");
            var segoe = new FontFamily("Segoe UI");
            var pattern = @"\[([^\]]+)\]\(([^)]+)\)";

            // The below works because consider the string "**Foo:** this is _my_ sentence."
            // it would get split into bold terms "", "Foo:", ...
            // with bold term [2] getting split into italicTerms " this is ", "my", " sentence."
            // Thus, if it's an odd index, that flag is live.

            var codeTerms = line.Split("`");
            for (int i = 0; i < codeTerms.Length; i++)
            {
                var isCode = i % 2 == 1;

                if (isCode)
                {
                    // Don't otherwise format code
                    inlines.Add(new Run(codeTerms[i])
                    {
                        FontSize = fontSize,
                        FontFamily = consolas
                    });
                }
                else
                {
                    var boldTerms = codeTerms[i].Split("**");
                    for (var j = 0; j < boldTerms.Length; j++)
                    {
                        var italicTerms = boldTerms[j].Split("_");
                        for (var k = 0; k < italicTerms.Length; k++)
                        {
                            // Strip out links
                            var term = Regex.Replace(italicTerms[k], pattern, "$1");

                            // Format
                            var isBold = j % 2 == 1;
                            var isItalics = k % 2 == 1;
                            var weight = isBold || forceBold ? FontWeights.Bold : FontWeights.Normal;
                            var style = isItalics || forceItalics ? FontStyles.Italic : FontStyles.Normal;

                            inlines.Add(new Run(term)
                            {
                                FontWeight = weight,
                                FontStyle = style,
                                FontSize = fontSize,
                                FontFamily = segoe
                            });
                        }
                    }
                }
            }
        }

        [GeneratedRegex("^(\\d+\\.|\\*|-) ")]
        private static partial Regex GetIsListRegex();
    }
}
