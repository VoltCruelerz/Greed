using Greed.Models;
using Greed.Models.EnabledMods;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greed
{
    static class ModLoader
    {
        public static List<Mod> LoadGreedyMods(string modDir)
        {
            var modDirs = Directory.GetDirectories(modDir);

            // If enabled path doesn't exist yet, make it.
            var enabledPath = modDir + "\\enabled_mods.json";
            List<string> enabledModFolders;
            if (File.Exists(enabledPath))
            {
                var enabled = JsonConvert.DeserializeObject<EnabledMods>(File.ReadAllText(enabledPath))!;
                enabledModFolders = enabled.ModKeys.Select(p => p.Name).ToList();
            } else
            {
                enabledModFolders = new List<string>();
                File.WriteAllText(enabledPath, "{ \"mod_keys\": [] }");
            }

            return modDirs.Select(d => new Mod(enabledModFolders, d)).Where(m => m.IsGreedy).ToList();
        }
    }
}
