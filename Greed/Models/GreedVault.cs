using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Greed.Interfaces;
using static System.Text.Json.JsonSerializer;

namespace Greed.Models
{
    public class GreedVault : IVault
    {
        private const string VaultName = "greed_vault.json";

        [JsonRequired]
        [JsonProperty(PropertyName = "active")]
        public List<string> Active { get; set; } = new();

        [JsonRequired]
        [JsonProperty(PropertyName = "packs")]
        public Dictionary<string, List<string>> Packs { get; set; } = new();

        public static GreedVault Load()
        {
            string modDir = ConfigurationManager.AppSettings["modDir"]!;
            var vaultPath = modDir + "\\" + VaultName;

            GreedVault pack;
            if (File.Exists(vaultPath))
            {
                var json = File.ReadAllText(vaultPath);
                pack = Deserialize<GreedVault>(json)!;
            }
            else
            {
                pack = new();
                pack.Export();
            }
            return pack;
        }

        public void Export()
        {
            string modDir = ConfigurationManager.AppSettings["modDir"]!;
            var vaultPath = modDir + "\\" + VaultName;

            File.WriteAllText(vaultPath, Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true
            }));
        }

        /// <summary>
        /// Saves the list of greedy mods to enabled_greed.json
        /// </summary>
        /// <param name="active"></param>
        public void ExportActiveOnly(List<Mod> allMods)
        {
            Active = allMods.Where(m => m.IsActive).Select(m => m.Id).ToList();
            Export();
        }

        public void UpsertPack(string name, List<Mod> allMods)
        {
            var active = allMods.Where(m => m.IsActive).Select(m => m.Id).ToList();
            Packs[name] = active;
            Export();
        }

        public void DeletePack(string name)
        {
            Packs.Remove(name);
            Export();
        }
    }
}
