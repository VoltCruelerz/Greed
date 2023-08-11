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
            } else
            {
                enabledModFolders = new List<string>();
                File.WriteAllText(enabledPath, "[]");
            }

            return modDirs.Select(d => new Mod(enabledModFolders, d)).Where(m => m.IsGreedy).ToList();
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
            // Burn down the old Greed mod directory (if exists)

            // Set Greed to be active mod #0

            // Copy gold JSON from Sins to the Greed mod dir.

            // For each greedy mod, overwrite as needed.
            active.Sort((a, b) => a.Meta.Priority -  b.Meta.Priority);
        }
    }
}
