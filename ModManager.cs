using Greed.Models;
using Greed.Models.EnabledMods;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greed
{
    static class ModManager
    {
        public static List<Mod> LoadGreedyMods(string modDir)
        {
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

        public static void SetGreedyMods(string modDir, List<Mod> active)
        {
            var arr = JArray.FromObject(active.Select(p => p.Id));
            var enabledPath = modDir + "\\enabled_greed.json";
            File.WriteAllText(enabledPath, arr.ToString());
        }
    }
}
