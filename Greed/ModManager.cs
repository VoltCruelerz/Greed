using Greed.Models;
using Greed.Models.EnabledMods;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System;

namespace Greed
{
    static class ModManager
    {
        public static List<Mod> LoadGreedyMods()
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

            return modDirs
                .Select(d => new Mod(enabledModFolders, d))
                .Where(m => m.IsGreedy)
                .OrderBy(m => m.Meta.Priority)
                .ToList();
        }

        public static void SetGreedyMods(List<Mod> active)
        {
            string modDir = ConfigurationManager.AppSettings["modDir"]!;
            var arr = JArray.FromObject(active.Select(p => p.Id));
            var enabledPath = modDir + "\\enabled_greed.json";
            File.WriteAllText(enabledPath, arr.ToString());
        }

        public static void ExportGreedyMods(List<Mod> active, ProgressBar pgbProgress, MainWindow window, Action<bool> callback)
        {
            Debug.WriteLine("Exporting Active Mods");
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
    }
}
