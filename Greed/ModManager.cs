using Greed.Controls;
using Greed.Controls.Popups;
using Greed.Exceptions;
using Greed.Extensions;
using Greed.Interfaces;
using Greed.Models;
using Greed.Models.Config;
using Greed.Models.EnabledMods;
using Greed.Models.Online;
using Greed.Models.Vault;
using Greed.Utils;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        public List<Mod> LoadGreedyMods(GreedVault vault)
        {
            string modDir = Settings.GetModDir();
            var modDirs = Directory.GetDirectories(modDir);

            var modIndex = 0;
            var mods = modDirs
                .Select(d => new Mod(vault, this, Warning, vault.Active, d, ref modIndex))
                .Where(m => m.IsGreedy)
                .ToList();
            mods.Sort((a, b) =>
            {
                var diff = a.LoadOrder.CompareTo(b.LoadOrder);
                if (diff != 0) return diff;

                var aLegal = a.Meta.IsLegalVersion().Any();
                var bLegal = b.Meta.IsLegalVersion().Any();
                if (aLegal && !bLegal) return 1;
                if (!aLegal && bLegal) return -1;

                return a.Meta.Name.CompareTo(b.Meta.Name);
            });
            return mods;
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
            Log.Info($"Exporting {active.Count} Active Mods");
            var slidersChanged = window.ScalarSliders.Any(s => s.HasChanged());
            string modDir = Settings.GetModDir();
            string sinsDir = Settings.GetSinsDir();
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
                int i = 0;
                try
                {
                    if (!active.Any() && !slidersChanged)
                    {
                        Log.Info("No active mods or scalars. Cleaning up...");
                        DeactivateGreed();
                        return;
                    }

                    // For each greedy mod, overwrite as needed.
                    for (i = 0; i < active.Count; i++)
                    {
                        var mod = active[i];
                        Log.Info($"[{i + 1}/{active.Count}]: Merging {mod.Meta.Name}...");
                        mod.Export(active);
                        pgbProgress.Dispatcher.Invoke(() =>
                        {
                            pgbProgress.Value = 100 * (i + 1) / active.Count;
                        });
                    }

                    // Set Global Settings
                    Settings.ExecGlobalScalars();

                    // Set Greed as active mod #0.
                    ActivateGreed();

                    callback(true);
                }
                catch (Exception ex)
                {
                    var cause = i < active.Count
                        ? active[i].ToString()
                        : "enabled_mods.json";
                    CriticalAlertPopup.ThrowAsync("Mod Export Error", new ModExportException("Failed to export during " + cause, ex));
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
        public void MoveMod(IVault vault, List<Mod> mods, Mod mover, int destination)
        {
            MoveModRecursive(mods, mover, destination);
            SyncLoadOrder(vault, mods);
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
            CanMove(mods, mover, destination, out List<Mod> dependencyViolations, out List<Mod> dependentViolations, out List<Mod> predViolations, out List<Mod> succViolations);
            var blockedByDependencies = dependencyViolations.Any();
            var blockedByDependents = dependentViolations.Any();
            var blockedByPredecessors = predViolations.Any();
            var blockedBySuccessors = succViolations.Any();

            var moveDependencies = false;
            if (blockedByDependencies)
            {
                var result = Warning.DependencyOrder(mover, dependencyViolations.Select(d => $"- Dependency Order Violation: {d.Meta.Name}").ToList());
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
                var result = Warning.DependentOrder(mover, dependentViolations.Select(d => $"- Dependent Order Violation: {d.Meta.Name}").ToList());
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

            var movePredecessors = false;
            if (blockedByPredecessors)
            {
                var result = Warning.PredecessorOrder(mover, predViolations.Select(d => $"- Predecessor Order Violation: {d.Meta.Name}").ToList());
                if (result == MessageBoxResult.Yes)
                {
                    blockedByPredecessors = false;
                    movePredecessors = true;
                }
                else if (result == MessageBoxResult.No)
                {
                    blockedByPredecessors = false;
                }
                else
                {
                    // Abort
                    return 0;
                }
            }

            var moveSuccessors = false;
            if (blockedBySuccessors)
            {
                var result = Warning.SuccessorOrder(mover, succViolations.Select(d => $"- Successor Order Violation: {d.Meta.Name}").ToList());
                if (result == MessageBoxResult.Yes)
                {
                    blockedBySuccessors = false;
                    moveSuccessors = true;
                }
                else if (result == MessageBoxResult.No)
                {
                    blockedBySuccessors = false;
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
            var movedSuccessorCount = 0;
            if (moveSuccessors)
            {
                succViolations.ForEach(v =>
                {
                    movedSuccessorCount += MoveModRecursive(mods, v, destination) + 1;
                });
            }
            if (!blockedByDependencies && !blockedByDependents && !blockedByPredecessors && !blockedBySuccessors)
            {
                var oldIndex = mods.IndexOf(mover);
                mods.RemoveAt(oldIndex);
                mods.Insert(Math.Clamp(destination - movedDescendentCount - movedSuccessorCount, 0, mods.Count), mover);
                Debug.WriteLine($"- Moved {mover} to {destination}");
            }
            if (movePredecessors)
            {
                predViolations.ForEach(v => MoveModRecursive(mods, v, destination));
            }
            if (moveDependencies)
            {
                dependencyViolations.ForEach(v => MoveModRecursive(mods, v, destination));
            }
            return movedDescendentCount + movedSuccessorCount;
        }

        public static void NewModWizard(OnlineCatalog catalog)
        {
            // Name
            var name = Interaction.InputBox("What would you like to name your mod?", "Greedy Wizard");
            if (string.IsNullOrEmpty(name)) return;

            var author = Interaction.InputBox("What is your username?", "Greedy Wizard");
            if (string.IsNullOrEmpty(author)) return;

            // Id
            var existingIds = catalog.Mods.Select(m => m.Id).ToHashSet();
            string id = "";
            do
            {
                if (id != "")
                {
                    MessageBox.Show("That id was taken. Please select another.", "Greedy Wizard", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                id = Interaction.InputBox("What would you like the id of your mod to be? It must be unique.", "Greedy Wizard");
                if (string.IsNullOrEmpty(id)) return;
            } while (existingIds.Contains(id));

            // Url
            var url = Interaction.InputBox("At what URL could players get more information or technical support for your mod?", "Greedy Wizard");
            if (string.IsNullOrEmpty(url)) return;

            // Description
            var desc = Interaction.InputBox("Please provide a brief summary of your mod's purpose.", "Greedy Wizard");
            if (string.IsNullOrEmpty(desc)) return;

            // Mod Version
            var version = new Version("1.0.0");

            // Sins Version
            var sinsVersion = Settings.GetSinsVersion();

            // Greed Version
            var greedVersion = Settings.GetGreedVersion();

            // Dependencies
            List<Dependency> deps = new();
            var depResponse = MessageBox.Show("Does your mod have dependencies?", "Greedy Wizard", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (depResponse == MessageBoxResult.Yes)
            {
                var depStr = Interaction.InputBox("What mods is yours dependent on? Please separate them with commas. (eg 'mod-a:1.0.0,other-mod:1.5.0'", "Greedy Wizard");
                if (string.IsNullOrEmpty(depStr)) return;

                var modStrs = depStr.Split(",");
                foreach (var item in modStrs)
                {
                    var terms = item.Split(":");
                    deps.Add(new Dependency
                    {
                        Id = terms[0].Trim(),
                        Version = new Version(terms[1].Trim())
                    });
                }
            }

            // Predecessors
            List<string> preds = new();
            var predResponse = MessageBox.Show("Does your mod have load order predecessors?", "Greedy Wizard", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (predResponse == MessageBoxResult.Yes)
            {
                var predStr = Interaction.InputBox("What mods should yours load after, if they are present? (eg 'mod-a,other-mod,mod-c'", "Greedy Wizard");
                if (string.IsNullOrEmpty(predStr)) return;

                preds = predStr.Split(",").Select(s => s.Trim()).ToList();
            }

            // Conflicts
            List<string> conflicts = new();
            var conflictsResponse = MessageBox.Show("Does your mod have conflicts?", "Greedy Wizard", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (conflictsResponse == MessageBoxResult.Yes)
            {
                var conflictsStr = Interaction.InputBox("What mods do you know yours will conflict with? (eg 'mod-a,other-mod,mod-c'", "Greedy Wizard");
                if (string.IsNullOrEmpty(conflictsStr)) return;

                conflicts = conflictsStr.Split(",").Select(s => s.Trim()).ToList();
            }

            // Total Conversion
            var isTotalConversion = MessageBox.Show("Is your mod a total conversion?", "Greedy Wizard", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

            // Generate mod
            var modDir = Settings.GetModDir();
            var modPath = Path.Combine(modDir, id);
            Directory.CreateDirectory(modPath);
            File.WriteAllText(Path.Combine(modPath, "greed.json"), JsonConvert.SerializeObject(new LocalInstall()
            {
                Name = name,
                Author = author,
                Description = desc,
                Url = url,
                Version = version,
                GreedVersion = greedVersion,
                SinsVersion = sinsVersion,
                Dependencies = deps,
                Predecessors = preds,
                Conflicts = conflicts,
                IsTotalConversion = isTotalConversion
            }, Formatting.Indented));

            // Open in Explorer
            var openResponse = MessageBox.Show("Mod template generation complete. Would you like to open file explorer to it?", "Greedy Wizard", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (openResponse == MessageBoxResult.Yes)
            {
                Process.Start("explorer.exe", modPath);
            }
        }

        /// <summary>
        /// Based on the order the mods are in the list, sets their load order index.
        /// </summary>
        /// <param name="mods"></param>
        public static void SyncLoadOrder(IVault vault, List<Mod> mods)
        {
            var active = mods.Where(m => m.IsActive).ToList();
            for (int i = 0; i < active.Count; i++)
            {
                var mod = active[i];
                mod.LoadOrder = i;
            }
            vault.ArchiveActiveOnly(active);
        }

        /// <summary>
        /// Set Greed itself as an active mod.
        /// </summary>
        private static void ActivateGreed()
        {
            if (!Directory.Exists(Settings.GetExportDir()))
            {
                Directory.CreateDirectory(Settings.GetExportDir());
            }
            // If enabled path doesn't exist yet, make it.
            var enabledPath = Settings.GetExportDir() + "\\enabled_mods.json";
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
        /// Remove Greed from the active mods list.
        /// </summary>
        private static void DeactivateGreed()
        {
            // If enabled path doesn't exist yet, make it.
            var greedPath = Settings.GetExportDir() + "\\greed";
            var enabledPath = Settings.GetExportDir() + "\\enabled_mods.json";

            if (File.Exists(enabledPath))
            {
                var enabled = JsonConvert.DeserializeObject<EnabledMods>(File.ReadAllText(enabledPath))!;
                enabled.ModKeys = enabled.ModKeys.Where(m => m.Name != "greed").ToList();
                Directory.Delete(greedPath, true);
                File.WriteAllText(enabledPath, JsonConvert.SerializeObject(enabled));
            }
        }

        /// <summary>
        /// Checks that the destination wouldn't break a dependency or predecessor chain.
        /// </summary>
        /// <param name="mods"></param>
        /// <param name="mover"></param>
        /// <param name="destination"></param>
        /// <param name="dependencyViolations"></param>
        /// <param name="dependentViolations"></param>
        /// <param name="predViolations"></param>
        /// <param name="succViolations"></param>
        /// <returns></returns>
        private static bool CanMove(List<Mod> mods, Mod mover, int destination, out List<Mod> dependencyViolations, out List<Mod> dependentViolations, out List<Mod> predViolations, out List<Mod> succViolations)
        {
            var oldIndex = mods.IndexOf(mover);
            var isLower = oldIndex < destination;
            var isHoist = oldIndex > destination;

            // Dependencies
            var dependencies = mover.Meta.GetDependencyMods(mods);

            dependencyViolations = !dependencies.Any()
                ? new List<Mod>()
                : dependencies
                    .Where(s => isHoist ? s.LoadOrder >= destination : s.LoadOrder > destination)
                    .ToList();

            var dependents = mods.Where(m => m.HasAsDirectDependency(mover));

            dependentViolations = !dependents.Any()
                ? new List<Mod>()
                : dependents
                    .Where(s => isLower ? s.LoadOrder <= destination : s.LoadOrder < destination)
                    .ToList();

            // Predecessors
            var preds = mover.Meta.GetPredecessorMods(mods);

            predViolations = !preds.Any()
                ? new List<Mod>()
                : preds
                    .Where(s => isHoist ? s.LoadOrder >= destination : s.LoadOrder > destination)
                    .ToList();

            var successors = mods.Where(m => m.HasAsDirectPredecessor(mover));

            succViolations = !successors.Any()
                ? new List<Mod>()
                : successors
                    .Where(s => isLower ? s.LoadOrder <= destination : s.LoadOrder < destination)
                    .ToList();

            return !dependencyViolations.Any() && !dependentViolations.Any()
                && !predViolations.Any() && !succViolations.Any();
        }

        /// <summary>
        /// Syncronously checks for the existing of the specified mod in the directory.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsModInstalled(string id)
        {
            string modDir = Settings.GetModDir();
            var modDirs = Directory.GetDirectories(modDir).Select(d => d.Split("\\")[^1]);
            return modDirs.Contains(id);
        }

        public static void Uninstall(MainWindow window, WarningPopup warning, List<Mod> installedMods, string id, bool force = false)
        {
            Log.Info($"Uninstalling {id}...");
            var modToUninstall = installedMods.Find(m => m.Id == id)!;

            if (force)
            {
                DeleteModFolder(id);
                window.ReloadModListFromDiskAsync();
                return;
            }

            var dependents = installedMods.Where(m => m.Meta.Dependencies.Select(d => d.Id).Contains(id));
            if (dependents.Any())
            {
                foreach (var dep in dependents)
                {
                    var response = warning.ChainedUninstall(modToUninstall, dep);
                    if (response == MessageBoxResult.Yes)
                    {
                        Uninstall(window, warning, installedMods, dep.Id);
                        DeleteModFolder(id);
                        window.ReloadModListFromDiskAsync();
                    }
                    else if (response == MessageBoxResult.No)
                    {
                        DeleteModFolder(id);
                        window.ReloadModListFromDiskAsync();
                    }
                    else
                    {
                        // Abort.
                        return;
                    }
                }
            }
            else
            {
                var response = MessageBox.Show($"Are you sure you want to uninstall {id}?", $"Uninstalling {id}", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (response == MessageBoxResult.Yes)
                {
                    DeleteModFolder(id);
                    window.ReloadModListFromDiskAsync();
                }
            }
        }

        private static void DeleteModFolder(string id)
        {
            string modDir = Settings.GetModDir();
            var path = modDir + "\\" + id;
            try
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
            catch (Exception ex)
            {
                // Sometimes Windows reports an error even if it did delete things, probably because of some recursive race somewhere.
                if (Directory.Exists(path))
                {
                    CriticalAlertPopup.Throw("Failed to delete mod at " + path, ex);
                }
            }
        }

        /// <summary>
        /// Installs a mod from the internet.
        /// 1. Validates presence of dependencies (unless forced).
        /// 2. Download file.
        /// 3. Unzip file.
        /// 4. Move file.
        /// 5. Cleanup temp files.
        /// </summary>
        /// <param name="window"></param>
        /// <param name="warning"></param>
        /// <param name="channel"></param>
        /// <param name="modToDownload"></param>
        /// <param name="versionToDownload"></param>
        /// <param name="force">If TRUE, ignore dependencies.</param>
        /// <returns>TRUE if success, FALSE if did not install.</returns>
        public static async Task<bool> InstallModFromInternet(MainWindow window, WarningPopup warning, OnlineCatalog channel, OnlineMod modToDownload, VersionEntry versionToDownload, bool force = false)
        {
            try
            {
                await window.SetProgressAsync(0);
                Log.Info($"Installing {modToDownload.Name}...");
                var url = versionToDownload.Download;

                if (!force && !await DependenciesReady(window, warning, channel, modToDownload, versionToDownload))
                {
                    Log.Warn($"Download of {modToDownload.Name} aborted.");
                    await window.SetProgressAsync(0);
                    return false;
                }
                await window.SetProgressAsync(0.1);

                // Download the file to the Downloads directory
                var filename = (modToDownload.Id + "_" + url.Split('/')[^1]).Split("?")[0];
                var zipPath = Path.Combine(Settings.GetDownDir(), filename);
                await window.SetProgressAsync(0.2);
                if (File.Exists(zipPath))
                {
                    File.Delete(zipPath);
                }
                if (!await IOManager.DownloadZipFile(url, zipPath))
                {
                    await window.SetProgressAsync(0);
                    return false;
                }
                Log.Info($"Download of {modToDownload.Name} to {zipPath} complete.");
                await window.SetProgressAsync(0.5);

                await InstallMod(window, zipPath, modToDownload!.Id);

                File.Delete(zipPath);
                await window.SetProgressAsync(1);
                return true;
            }
            catch (Exception ex)
            {
                CriticalAlertPopup.Throw("Failed to Download Mod", ex);
                await window.SetProgressAsync(0);
                return false;
            }
        }

        public static async Task<bool> DependenciesReady(MainWindow window, WarningPopup warning, OnlineCatalog channel, OnlineMod onlineMod, VersionEntry desiredVersion)
        {
            foreach (var dep in desiredVersion.Dependencies)
            {
                if (dep.IsOutdatedOrMissing())
                {
                    var result = warning.ChainedInstall(onlineMod, dep);
                    if (result == MessageBoxResult.Cancel)
                    {
                        return false;
                    }
                    else if (result == MessageBoxResult.Yes)
                    {
                        var dependencyMod = channel.Mods.Find(m => m.Id == dep.Id);
                        var versionToInstall = dependencyMod!.Versions[dep.Version.ToString()];
                        var chainResult = await InstallModFromInternet(window, warning, channel, dependencyMod, versionToInstall);
                        if (!chainResult)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        // No means do nothing.
                    }
                }
            }
            return true;
        }

        public static async Task InstallMod(MainWindow window, string archivePath, string modId = "")
        {
            try
            {
                var extension = Path.GetExtension(archivePath);

                // Extract the mod.
                var extractPath = archivePath.Split(extension)[0];
                if (Directory.Exists(extractPath))
                {
                    Directory.Delete(extractPath, true);
                }
                Log.Info($"Extracting to {extractPath}...");
                try
                {
                    IOManager.ExtractArchive(archivePath, extractPath);
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                    await window.SetProgressAsync(0.0);
                    return;
                }
                await window.SetProgressAsync(0.8);
                Log.Info($"Extract complete.");

                // Check if the mod is shallow or nested, and get the folder we'll want to copy.
                var isShallow = File.Exists(extractPath + "\\greed.json");
                var copyablePath = isShallow
                    ? extractPath
                    : Directory.GetDirectories(extractPath)[0];

                // Check that greed.json actually exists. The mod might not be greedy, after all.
                if (string.IsNullOrEmpty(modId))
                {
                    var hypotheticalId = copyablePath.Split("\\")[^1];
                    Log.Warn("Mod ID of auto-imported mod inferred as " + hypotheticalId);
                    modId = hypotheticalId;
                }


                // Shift to the mod directory
                var modPath = Settings.GetModDir() + "\\" + modId;
                if (Directory.Exists(modPath))
                {
                    IOManager.ReadyDirForDelete(modPath);
                    Directory.Delete(modPath, true);
                    Log.Info($"Deleted old install to make way for new one.");
                }
                if (!Directory.Exists(copyablePath))
                {
                    Log.Info($"Folder \"{copyablePath}\" doesn't exist yet!");
                }
                await window.SetProgressAsync(0.8);
                Log.Info($"Starting move from {copyablePath}...");
                await IOManager.MoveDirWithRetries(copyablePath, modPath);
                Log.Info($"Move complete.");
                Log.Info($"Install complete.");
                await window.SetProgressAsync(0.9);

                // Cleanup
                if (!isShallow)
                {
                    Directory.Delete(extractPath, true);
                }
            }
            catch (Exception ex)
            {
                CriticalAlertPopup.Throw("Failed to Install Mod", ex);
            }
        }
    }
}
