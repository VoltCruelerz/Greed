﻿using Greed.Interfaces;
using Greed.Models.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using FontFamily = System.Windows.Media.FontFamily;
using Greed.Exceptions;
using SharpCompress.Compressors.Xz;

namespace Greed.Models
{
    public partial class Mod
    {
        private LocalInstall? Metadata { get; set; }
        private readonly IVault Vault;
        private readonly IModManager Manager;
        private readonly IWarningPopup Warning;

        public LocalInstall Meta => Metadata!;
        public bool IsActive { get; private set; } = false;
        public string Readme { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public bool IsGreedy => Metadata != null;
        public int LoadOrder = -1;

        #region Folder Contents
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
        #endregion

        public Mod(IVault vault, IModManager manager, IWarningPopup warning, List<string> enabledModIds, string path, ref int modIndex)
        {
            Debug.WriteLine("Loading " + path);
            Vault = vault;
            Manager = manager;
            Warning = warning;

            try
            {
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
                LoadOrder = enabledModIds.IndexOf(Id);
                if (LoadOrder != -1)
                {
                    IsActive = true;
                    modIndex++;
                }
                else
                {
                    // Stick it at the bottom of the list.
                    LoadOrder = int.MaxValue;
                }

                Metadata = JsonConvert.DeserializeObject<LocalInstall>(File.ReadAllText(greedMetaFilename));

                var subpaths = Directory.GetDirectories(path);
                var subdirs = subpaths.Select(p => p[(path.Length + 1)..]);

                // Json data
                Brushes = ImportJsonFolder(subdirs, path, "brushes", (p) => new JsonSource(p));
                Colors = ImportJsonFolder(subdirs, path, "colors", (p) => new JsonSource(p));
                Cursors = ImportJsonFolder(subdirs, path, "cursors", (p) => new JsonSource(p));
                DeathSequences = ImportJsonFolder(subdirs, path, "death_sequences", (p) => new JsonSource(p));
                Effects = ImportJsonFolder(subdirs, path, "effects", (p) => new JsonSource(p));
                Entities = ImportJsonFolder(subdirs, path, "entities", (p) => new JsonSource(p));
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
                LocalizedTexts = ImportJsonFolder(subdirs, path, "localized_text", (p) => new JsonSource(p));

                // Non-JSON
                Sounds = ImportFolder(subdirs, path, "sounds");
                Shaders = ImportFolder(subdirs, path, "shaders");
                Scenarios = ImportFolder(subdirs, path, "scenarios");
                Meshes = ImportFolder(subdirs, path, "meshes");
                Textures = ImportFolder(subdirs, path, "textures");
            } catch (Exception ex)
            {
                throw new ModLoadException("Failed to load mod at " + path, ex);
            }
        }

        private static List<JsonSource> ImportJsonFolder(IEnumerable<string> subdirs, string path, string folder, Func<string, JsonSource> handleFileImport)
        {
            if (subdirs.Contains(folder))
            {
                var sources = Directory.GetFiles(path + "\\" + folder)
                    .Select(p => handleFileImport(p))
                    .ToList();

                // Sort ASC: export order -> file name.
                sources.Sort((a, b) => a.CompareTo(b));
                return sources;
            }
            return new List<JsonSource>();
        }

        private static List<Source> ImportFolder(IEnumerable<string> subdirs, string path, string folder)
        {
            if (subdirs.Contains(folder))
            {
                var sources = Directory.GetFiles(path + "\\" + folder)
                    .Select(p => new Source(p))
                    .ToList();
                sources.Sort((a, b) => a.Filename.CompareTo(b.Filename));
                return sources;
            }
            return new List<Source>();
        }

        public void Export(List<Mod> active)
        {
            Debug.WriteLine("- Exporting " + Id);

            // Merge Json
            ExportJsonFolder(Brushes, active);
            ExportJsonFolder(Colors, active);
            ExportJsonFolder(Cursors, active);
            ExportJsonFolder(DeathSequences, active);
            ExportJsonFolder(Effects, active);
            ExportJsonFolder(Entities, active);
            ExportJsonFolder(Fonts, active);
            ExportJsonFolder(GravityWellProps, active);
            ExportJsonFolder(Gui, active);
            ExportJsonFolder(MeshMaterials, active);
            ExportJsonFolder(PlayerColors, active);
            ExportJsonFolder(PlayerIcons, active);
            ExportJsonFolder(PlayerPortraits, active);
            ExportJsonFolder(Skyboxes, active);
            ExportJsonFolder(TextureAnimations, active);
            ExportJsonFolder(Uniforms, active);
            ExportJsonFolder(LocalizedTexts, active);

            // Overwrite binary types
            ExportSourceFolder(Meshes);
            ExportSourceFolder(Scenarios);
            ExportSourceFolder(Shaders);
            ExportSourceFolder(Sounds);
            ExportSourceFolder(Textures);
        }

        private static void ExportJsonFolder(List<JsonSource> sources, List<Mod> active)
        {
            foreach (var source in sources)
            {
                // If the source has prerequisites, check that they're on first.
                var activeIds = active.Select(m => m.Id).ToHashSet();
                var prereqs = source.GreedRules?.Prerequisites ?? new List<string>();
                if (!prereqs.All(p => activeIds.Contains(p)))
                {
                    Debug.WriteLine("- - - Skipping " + source.Filename);
                    continue;
                }

                Debug.WriteLine("- - " + source.Filename);
                CreateGreedDirIfNotExists(source.GreedPath);

                var greedExists = File.Exists(source.GreedPath);
                var parentExists = File.Exists(source.ParentGreedPath);
                var goldExists = File.Exists(source.GoldPath);

                // If the file has a parent that isn't in greed yet, copy it over.
                if (!greedExists && !parentExists && source.ParentGreedPath != null)
                {
                    // The gold path for those with parents is that of the parent.
                    File.Copy(source.GoldPath, source.ParentGreedPath);
                    parentExists = true;
                }

                // If greed doesn't exist yet, but the parent does, initialize from parent
                if (!greedExists && parentExists)
                {
                    File.Copy(source.ParentGreedPath!, source.GreedPath);
                    greedExists = true;
                }

                // If greed doesn't exist yet, initialize it from gold as needed.
                if (!greedExists && goldExists && source.RequiresGold())
                {
                    File.Copy(source.GoldPath, source.GreedPath);
                    greedExists = true;
                }

                // If greed exists, we need to merge the source into it.
                var output = greedExists
                    ? new JsonSource(source.GreedPath).Merge(source)
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
                if (File.Exists(source.GreedPath))
                {
                    File.Copy(source.SourcePath, source.GreedPath, true);
                }
                else
                {
                    File.Copy(source.SourcePath, source.GreedPath);
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

        public string DiffFromGold(List<Mod> active)
        {
            var sb = new StringBuilder();
            sb.AppendLine(Id);
            Entities.ForEach(p => sb.AppendLine("Entity " + p.DiffFromGold(active)));
            LocalizedTexts.ForEach(p => sb.AppendLine("Localized Text " + p.DiffFromGold(active)));

            return sb.ToString();
        }

        public void RenderReadme(BlockCollection blocks)
        {
            var listRegex = GetIsListRegex();
            var noSpace = new Thickness(0);
            var lines = Readme.Split(Environment.NewLine)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();

            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                var trimmed = line.Trim();
                if (line.StartsWith("#"))
                {
                    var boost = 2 * (5 - line.Count(c => c == '#'));
                    var titleText = line[(line.IndexOf(" ") + 1)..];
                    var p = new Paragraph();
                    FormatLine(p.Inlines, titleText, 10 + boost, true);
                    blocks.Add(p);
                }
                else if (trimmed.StartsWith("*") || trimmed.StartsWith("-"))
                {
                    var p = new Paragraph();
                    FormatLine(p.Inlines, line + Environment.NewLine);
                    for (int j = i + 1; j < lines.Count; j++)
                    {
                        var trimJ = lines[j].Trim();
                        if (GetIsListRegex().IsMatch(trimJ))
                        {
                            FormatLine(p.Inlines, lines[j] + Environment.NewLine);
                            i++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    blocks.Add(p);
                }
                else if (trimmed.StartsWith("|") && trimmed.EndsWith("|"))
                {
                    Table table = new()
                    {
                        Background = new SolidColorBrush(Color.FromRgb(255, 255, 255))
                    };
                    table.RowGroups.Add(new TableRowGroup());
                    var offset = 0;
                    while (i < lines.Count)
                    {
                        // Skip the row that is just the horizontal bars
                        if (offset != 1)
                        {
                            line = lines[i];
                            trimmed = line.Trim();
                            var isTableLine = trimmed.StartsWith("|") && trimmed.EndsWith("|");
                            TableRow row = new();
                            if (offset % 2 == 1 || offset == 0)
                            {
                                row.Background = new SolidColorBrush(Color.FromRgb(200, 200, 255));
                            }
                            var cells = trimmed.Split("|");

                            // Ignore the 0th and last term because those are the border.
                            for (var j = 1; j < cells.Length - 1; j++)
                            {
                                var cellTrimmed = cells[j].Trim();
                                var cellP = new Paragraph();
                                FormatLine(cellP.Inlines, cellTrimmed, 10, offset == 0);
                                row.Cells.Add(new TableCell(cellP));
                            }
                            table.RowGroups[0].Rows.Add(row);
                        }
                        i++;
                        offset++;
                    }
                    blocks.Add(table);
                }
                else
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
            var pattern = @"!?\[([^\]]+)\]\(([^)]+)\)";

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

        public override string ToString()
        {
            return Id;
        }

        public void SetModActivity(List<Mod> allMods, bool willBeActive, bool force)
        {
            if (force)
            {
                IsActive = willBeActive;
            }
            else
            {
                SetModActivity(allMods, willBeActive);
            }
        }

        public void SetModActivity(List<Mod> allMods, bool willBeActive)
        {
            var activeMods = allMods.Where(mod => mod.IsActive).ToList();

            if (!willBeActive)
            {
                // Check if any other active mods need this mod
                var dependents = activeMods.Where(m => m.Meta.Dependencies.Any(d => d.Id == this.Id)).ToList();

                if (dependents.Any())
                {
                    var depResponse = Warning.Dependents(this, dependents);
                    if (depResponse == MessageBoxResult.Yes)
                    {
                        dependents.ForEach(d => d.SetModActivity(allMods, false));
                    }
                    else if (depResponse == MessageBoxResult.No)
                    {
                        // Do nothing. Trust the user to know what they're doing.
                    }
                    else
                    {
                        return;
                    }
                }

                IsActive = false;
                Vault.ArchiveActiveOnly(allMods);
                return;
            }

            // Make sure all our dependencies are turned on.
            var dependencyViolations = Meta.GetDependencyViolations(allMods);
            var violationText = dependencyViolations.Item1;
            var inactiveDependencies = dependencyViolations.Item2;
            if (violationText.Any())
            {
                var depResponse = Warning.Dependencies(this, violationText, inactiveDependencies);
                if (depResponse == MessageBoxResult.Yes)
                {
                    inactiveDependencies.ForEach(d =>
                    {
                        d.SetModActivity(allMods, true);
                        // Recalculate the dependency violations
                        dependencyViolations = Meta.GetDependencyViolations(allMods);
                        violationText = dependencyViolations.Item1;
                        inactiveDependencies = dependencyViolations.Item2;
                    });

                    // Unable to resolve all dependencies.
                    if (violationText.Count > 0)
                    {
                        Warning.FailedToResolveDependencies(violationText);
                        return;
                    }
                }
                else if (depResponse == MessageBoxResult.No)
                {
                    // Do nothing. Trust the user to know what they're doing.
                }
                else
                {
                    // Abort.
                    return;
                }
            }

            var preds = Meta.GetPredecessors().ToHashSet();

            // Make sure we load after our predecessors and dependencies.

            var lowestLegalIndex = Math.Min(
                Meta.GetDependencyMods(allMods).Select(d => d.LoadOrder).DefaultIfEmpty(activeMods.Count - 1).Min(),
                Meta.GetPredecessorMods(allMods).Select(d => d.LoadOrder).DefaultIfEmpty(activeMods.Count - 1).Min()
            );
            if (LoadOrder < lowestLegalIndex)
            {
                Manager.MoveMod(Vault, allMods, this, lowestLegalIndex);
            }

            // If we're turning a mod on, we need to check for conflicts in both directions because one mod might not know about the other.
            var conflicts = activeMods.Where(IsConflict);

            if (!conflicts.Any())
            {
                IsActive = true;
                Vault.ArchiveActiveOnly(allMods);
                return;
            }

            var conResponse = Warning.Conflicts(this, conflicts.ToList());

            if (conResponse == MessageBoxResult.Yes)
            {
                foreach (var c in conflicts)
                {
                    c.IsActive = false;
                }
                IsActive = true;
            }
            else if (conResponse == MessageBoxResult.No)
            {
                IsActive = true;
            }
            else
            {
                // Abort. Do nothing.
            }
            Vault.ArchiveActiveOnly(allMods);
        }

        public bool HasDirectDependent(Mod potentialDependent)
        {
            return potentialDependent.Meta.Dependencies.Any(d => d.Id == Id);
        }

        public bool HasAsDirectDependency(Mod potentialDependency)
        {
            return potentialDependency.HasDirectDependent(this);
        }

        public bool HasAsDirectPredecessor(Mod potentialPredecessor)
        {
            return Meta.GetPredecessors().Any(p => p == potentialPredecessor.Id);
        }

        public bool IsConflict(Mod other)
        {
            if (other == null) return false;

            // Check basic conflicts first.
            if (other.Meta.Conflicts.Contains(Id) || Meta.Conflicts.Contains(other.Id))
            {
                return true;
            }

            // Check total conversion
            if (Meta.IsTotalConversion && !other.Meta.Dependencies.Select(d => d.Id).Contains(Id))
            {
                return true;
            }
            if (other.Meta.IsTotalConversion && !Meta.Dependencies.Select(d => d.Id).Contains(other.Id))
            {
                return true;
            }

            // Default to false.
            return false;
        }
    }
}
