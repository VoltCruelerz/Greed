using Greed.Controls;
using Greed.Interfaces;
using Greed.Models;
using Greed.Models.EnabledMods;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Greed
{
    public class ModManager : IModManager
    {
        private readonly IWarningPopup Warning;

        /// <summary>
        /// Main constructor.
        /// </summary>
        public ModManager()
        {
            Warning = new WarningPopup();
        }

        /// <summary>
        /// For unit testing.
        /// </summary>
        /// <param name="popup"></param>
        public ModManager(IWarningPopup popup)
        {
            Warning = popup;
        }

        /// <summary>
        /// Loads all Greedy mods from the mods directory.
        /// </summary>
        /// <returns></returns>
        public List<Mod> LoadGreedyMods()
        {
            string modDir = ConfigurationManager.AppSettings["modDir"]!;
            var modDirs = Directory.GetDirectories(modDir);

            // If enabled path doesn't exist yet, make it.
            var enabledPath = modDir + "\\enabled_greed.json";
            List<string> enabledModFolders;
            if (File.Exists(enabledPath))
            {
                enabledModFolders = JArray.Parse(File.ReadAllText(enabledPath)).Select(p => p.ToString()).ToList();
            }
            else
            {
                enabledModFolders = new List<string>();
                File.WriteAllText(enabledPath, "[]");
            }

            var modIndex = 0;
            return modDirs
                .Select(d => new Mod(this, Warning, enabledModFolders, d, ref modIndex))
                .Where(m => m.IsGreedy)
                .OrderBy(m => m.LoadOrder)
                .ToList();
        }

        /// <summary>
        /// Saves the list of greedy mods to enabled_greed.json
        /// </summary>
        /// <param name="active"></param>
        public void SetGreedyMods(List<Mod> active)
        {
            string modDir = ConfigurationManager.AppSettings["modDir"]!;
            var arr = JArray.FromObject(active.Select(p => p.Id));
            var enabledPath = modDir + "\\enabled_greed.json";
            File.WriteAllText(enabledPath, arr.ToString());
        }

        /// <summary>
        /// Exports the list of active mods to a freshly regenerated mods/greed.
        /// </summary>
        /// <param name="active"></param>
        /// <param name="pgbProgress"></param>
        /// <param name="window"></param>
        /// <param name="callback"></param>
        public void ExportGreedyMods(List<Mod> active, ProgressBar pgbProgress, MainWindow window, Action<bool> callback)
        {
            window.PrintAsync($"Exporting {active.Count} Active Mods");
            string modDir = ConfigurationManager.AppSettings["modDir"]!;
            string sinsDir = ConfigurationManager.AppSettings["sinsDir"]!;
            var greedPath = modDir + "\\greed";

            // Burn down the old Greed mod directory (if exists) and make it anew.
            if (Directory.Exists(greedPath))
            {
                Directory.Delete(greedPath, true);
            }
            Directory.CreateDirectory(greedPath);
            File.WriteAllText(greedPath + "\\mod_manifest.json", "{ \"is_cosmetic_only\": false }");

            Task.Run(() =>
            {
                if (!active.Any())
                {
                    window.PrintAsync("No active mods.");
                    return;
                }
                try
                {
                    // For each greedy mod, overwrite as needed.
                    var interval = 100 / active.Count;
                    for (int i = 0; i < active.Count; i++)
                    {
                        var mod = active[i];
                        window.PrintAsync($"[{i + 1}/{active.Count}]: Merging {mod.Meta.Name}...");
                        mod.Export();
                        pgbProgress.Dispatcher.Invoke(() =>
                        {
                            pgbProgress.Value = 100 * (i + 1) / active.Count;
                        });
                    }
                    // Set Greed as active mod #0.
                    ActivateGreed();

                    callback(true);
                }
                catch (Exception ex)
                {
                    window.PrintAsync("Error: " + ex.ToString());
                    callback(false);
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Moves the target mod to the destination point if possible and exports the new load order.
        /// </summary>
        /// <param name="mods">all mods</param>
        /// <param name="mover">the mod to move</param>
        /// <param name="destination">the destination index in the list</param>
        public void MoveMod(List<Mod> mods, Mod mover, int destination)
        {
            MoveModRecursive(mods, mover, destination);
            SyncLoadOrder(mods);
        }

        /// <summary>
        /// Moves the target mod to the destination point if possible, and (based on user input) may hoist its dependencies as needed.
        /// </summary>
        /// <param name="mods">all mods</param>
        /// <param name="mover">the mod to move</param>
        /// <param name="destination">the destination index in the list</param>
        /// <returns>TRUE if moved, FALSE if failed to move.</returns>
        private int MoveModRecursive(List<Mod> mods, Mod mover, int destination)
        {
            // Check to make sure we're not already there.
            if (mods.IndexOf(mover) == destination)
            {
                return 0;
            }

            Debug.WriteLine($"Attempting to move {mover} to {destination} for list [{string.Join(", ", mods)}]");
            CanMove(mods, mover, destination, out List<Mod> dependencyViolations, out List<Mod> dependentViolations);
            var blockedByDependencies = dependencyViolations.Any();
            var blockedByDependents = dependentViolations.Any();

            var moveDependencies = false;
            if (blockedByDependencies)
            {
                var result = Warning.WarnOfDependencyOrder(mover, dependencyViolations.Select(d => $"- Dependency Order Violation: {d.Meta.Name}").ToList());
                if (result == MessageBoxResult.Yes)
                {
                    blockedByDependencies = false;
                    moveDependencies = true;
                }
                else if (result == MessageBoxResult.No)
                {
                    blockedByDependencies = false;
                }
                else
                {
                    // Abort
                    return 0;
                }
            }

            var moveDependents = false;
            if (blockedByDependents)
            {
                var result = Warning.WarnOfDependencyOrder(mover, dependentViolations.Select(d => $"- Dependent Order Violation: {d.Meta.Name}").ToList());
                if (result == MessageBoxResult.Yes)
                {
                    blockedByDependents = false;
                    moveDependents = true;
                }
                else if (result == MessageBoxResult.No)
                {
                    blockedByDependents = false;
                }
                else
                {
                    // Abort
                    return 0;
                }
            }

            // Because we're inserting at a set point, we insert dependents first, then the mover, and finally the mover's dependencies.
            // This gives us the order of dependencies -> mover -> dependents
            var movedDescendentCount = 0;
            if (moveDependents)
            {
                dependentViolations.ForEach(v =>
                {
                    movedDescendentCount += MoveModRecursive(mods, v, destination) + 1;
                });
            }
            if (!blockedByDependencies && !blockedByDependents)
            {
                var oldIndex = mods.IndexOf(mover);
                mods.RemoveAt(oldIndex);
                mods.Insert(destination - movedDescendentCount, mover);
                Debug.WriteLine($"- Moved {mover} to {destination}");
            }
            if (moveDependencies)
            {
                dependencyViolations.ForEach(v => MoveModRecursive(mods, v, destination));
            }
            return movedDescendentCount;
        }

        /// <summary>
        /// Based on the order the mods are in the list, sets their load order index.
        /// </summary>
        /// <param name="mods"></param>
        public void SyncLoadOrder(List<Mod> mods)
        {
            var active = mods.Where(m => m.IsActive).ToList();
            for (int i = 0; i < active.Count; i++)
            {
                var mod = active[i];
                mod.LoadOrder = i;
            }
            SetGreedyMods(active);
        }

        /// <summary>
        /// Set Greed itself as an active mod.
        /// </summary>
        private static void ActivateGreed()
        {
            // If enabled path doesn't exist yet, make it.
            var enabledPath = ConfigurationManager.AppSettings["modDir"]! + "\\enabled_mods.json";
            EnabledMods enabled;
            var greedKey = new ModKey()
            {
                Name = "greed",
                Version = 0// Sins wants this to be an int for whatever reason.
            };

            if (File.Exists(enabledPath))
            {
                enabled = JsonConvert.DeserializeObject<EnabledMods>(File.ReadAllText(enabledPath))!;
                if (enabled.ModKeys.Any(mk => mk.Name == "greed"))
                {
                    // No update required.
                    return;
                }
                enabled.ModKeys.Add(greedKey);
            }
            else
            {
                enabled = new EnabledMods
                {
                    ModKeys = new List<ModKey>()
                    {
                        greedKey
                    }
                };
            }
            File.WriteAllText(enabledPath, JsonConvert.SerializeObject(enabled));
        }

        /// <summary>
        /// Checks that the destination wouldn't break a dependency chain.
        /// </summary>
        /// <param name="mods"></param>
        /// <param name="mover"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        private static bool CanMove(List<Mod> mods, Mod mover, int destination, out List<Mod> dependencyViolations, out List<Mod> dependentViolations)
        {
            var oldIndex = mods.IndexOf(mover);
            var dependencies = mover.Meta.GetDependencyMods(mods);

            dependencyViolations = !dependencies.Any()
                ? new List<Mod>()
                : dependencies
                    .Where(d => d.LoadOrder >= destination)
                    .ToList();

            var dependents = mods
                .Where(m => m.HasDirectDependency(mover));

            dependentViolations = !dependents.Any()
                ? new List<Mod>()
                : dependents
                    .Where(d => d.LoadOrder <= destination)
                    .ToList();

            return !dependencyViolations.Any();
        }
    }
}
