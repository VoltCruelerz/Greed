using Greed.Controls;
using Greed.Interfaces;
using Greed.Models;
using Greed.Models.EnabledMods;
using Greed.Models.Online;
using Greed.Models.Vault;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading;
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
            string modDir = ConfigurationManager.AppSettings["modDir"]!;
            var modDirs = Directory.GetDirectories(modDir);

            var modIndex = 0;
            return modDirs
                .Select(d => new Mod(vault, this, Warning, vault.Active, d, ref modIndex))
                .Where(m => m.IsGreedy)
                .OrderBy(m => m.LoadOrder)
                .ToList();
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
            CanMove(mods, mover, destination, out List<Mod> dependencyViolations, out List<Mod> dependentViolations);
            var blockedByDependencies = dependencyViolations.Any();
            var blockedByDependents = dependentViolations.Any();

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
                var result = Warning.DependencyOrder(mover, dependentViolations.Select(d => $"- Dependent Order Violation: {d.Meta.Name}").ToList());
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

        /// <summary>
        /// Syncronously checks for the existing of the specified mod in the directory.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsModInstalled(string id)
        {
            string modDir = ConfigurationManager.AppSettings["modDir"]!;
            var modDirs = Directory.GetDirectories(modDir).Select(d => d.Split("\\")[^1]);
            return modDirs.Contains(id);
        }

        public static void Uninstall(MainWindow window, WarningPopup warning, List<Mod> installedMods, string id, bool force = false)
        {
            window.PrintAsync($"Uninstalling {id}...");
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
            string modDir = ConfigurationManager.AppSettings["modDir"]!;
            var path = modDir + "\\" + id;
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        /// <summary>
        /// Installs a mod from github.
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
        public static async Task<bool> InstallModFromGitHub(MainWindow window, WarningPopup warning, OnlineCatalog channel, OnlineMod modToDownload, VersionEntry versionToDownload, bool force = false)
        {
            try
            {
                window.PrintAsync($"Installing {modToDownload.Name}...");
                var url = versionToDownload.Download;

                if (!force && !await DependenciesReady(window, warning, channel, modToDownload, versionToDownload))
                {
                    window.PrintAsync($"Download of {modToDownload.Name} aborted.");
                    return false;
                }

                // Download the file to the Downloads directory
                var filename = modToDownload.Id + "_" + url.Split('/')[^1];
                var zipPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "Downloads",
                    filename
                );
                if (File.Exists(zipPath))
                {
                    File.Delete(zipPath);
                }
                if (!await DownloadZipFile(window, url, zipPath))
                {
                    return false;
                }
                window.PrintAsync($"Download of {modToDownload.Name} to {zipPath} complete.");

                // Extract the mod.
                var extractPath = zipPath.Split(".zip")[0];
                if (Directory.Exists(extractPath))
                {
                    Directory.Delete(extractPath, true);
                }
                window.PrintAsync($"Extracting to {extractPath}...");
                ZipFile.ExtractToDirectory(zipPath, extractPath);
                window.PrintAsync($"Extract complete.");

                // Shift to the mod directory
                var internalDir = Directory.GetDirectories(extractPath)[0];
                var modPath = ConfigurationManager.AppSettings["modDir"]! + "\\" + modToDownload!.Id;
                if (Directory.Exists(modPath))
                {
                    Directory.Delete(modPath, true);
                    window.PrintAsync($"Deleted old install to make way for new one.");
                }
                if (!Directory.Exists(internalDir))
                {
                    window.PrintAsync($"Folder doesn't exist yet!");
                }
                window.PrintAsync($"Starting move from {internalDir}...");
                MoveWithRetries(window, internalDir, modPath, 3);
                window.PrintAsync($"Move complete.");
                window.PrintAsync($"Install complete.");

                // Cleanup
                File.Delete(zipPath);
                Directory.Delete(extractPath, true);
                return true;
            }
            catch (Exception ex)
            {
                window.CriticalAlertPopup("Failed to Download Mod", ex);
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
                        var chainResult = await InstallModFromGitHub(window, warning, channel, dependencyMod, versionToInstall);
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

        public static async Task<bool> DownloadZipFile(MainWindow window, string releaseUrl, string outputPath)
        {
            using HttpClient httpClient = new();
            try
            {
                // Send an HTTP GET request to the GitHub release URL
                window.PrintAsync("Downloading from " + releaseUrl);
                HttpResponseMessage response = await httpClient.GetAsync(releaseUrl);

                // Check if the request was successful (HTTP status code 200)
                if (response.IsSuccessStatusCode)
                {
                    // Get the response stream and create a FileStream to save the .zip file
                    using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                    using (FileStream fileStream = File.Create(outputPath))
                    {
                        // Copy the content stream to the file stream
                        await contentStream.CopyToAsync(fileStream);
                    }

                    window.PrintAsync("Download completed successfully.");
                }
                else
                {
                    throw new InvalidOperationException($"Failed to download. HTTP status code: {response.StatusCode}");
                }
                return true;
            }
            catch (Exception ex)
            {
                window.PrintAsync($"A download error occurred: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Repeatedly attempts to move a directory that may or may not be locked down by ZipFile.ExtractToDirectory()
        /// </summary>
        /// <param name="window"></param>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <param name="maxRetries"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private static void MoveWithRetries(MainWindow window, string src, string dest, int maxRetries = 5)
        {
            for (var retry = 0; retry <= maxRetries; retry++)
            {
                if (Directory.Exists(src))
                {
                    try
                    {
                        Directory.Move(src, dest);
                        break;
                    }
                    catch (Exception ex)
                    {
                        window.PrintAsync(ex);
                        Thread.Sleep(retry * 100);
                        window.PrintAsync($"Retrying move (r={retry})");
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Internal extracted directory does not exist!");
                }
            }
        }
    }
}
