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
using System.Text;
using System.Threading.Tasks;

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

        public static void ExportGreedyMods(List<Mod> active)
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

            // For each greedy mod, overwrite as needed.
            active.ForEach(m => m.Export());

            // Set Greed as active mod #0.
            ActivateGreed();
        }

        private static void ActivateGreed()
        {
            // If enabled path doesn't exist yet, make it.
            var enabledPath = ConfigurationManager.AppSettings["modDir"]! + "\\enabled_mods.json";
            EnabledMods enabled;
            var greedKey = new ModKey()
            {
                Name = "greed",
                Version = int.Parse(ConfigurationManager.AppSettings["greedVersion"]!)
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
