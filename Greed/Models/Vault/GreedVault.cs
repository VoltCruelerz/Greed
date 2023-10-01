using Greed.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.Json;
using static System.Text.Json.JsonSerializer;

namespace Greed.Models.Vault
{
    public class GreedVault : IVault
    {
        private const string VaultName = "greed_vault.json";
        private static readonly JsonSerializerOptions SeriOptions = new()
        {
            WriteIndented = true
        };

        [JsonRequired]
        [JsonProperty(PropertyName = "active")]
        public List<string> Active { get; set; } = new();

        [JsonRequired]
        [JsonProperty(PropertyName = "packs")]
        public Dictionary<string, List<string>> Packs { get; set; } = new();

        public static GreedVault Load()
        {
            string modDir = ConfigurationManager.AppSettings["modDir"]!;
            if (!Directory.Exists(modDir))
            {
                return new();
            }
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
                pack.Archive();
            }
            return pack;
        }

        private void Archive()
        {
            string modDir = ConfigurationManager.AppSettings["modDir"]!;
            if (!Directory.Exists(modDir))
            {
                return;
            }
            var vaultPath = modDir + "\\" + VaultName;

            File.WriteAllText(vaultPath, Serialize(this, SeriOptions));
        }

        /// <summary>
        /// Saves the list of greedy mods to enabled_greed.json
        /// </summary>
        /// <param name="active"></param>
        public void ArchiveActiveOnly(List<Mod> allMods)
        {
            Active = allMods.Where(m => m.IsActive).Select(m => m.Id).ToList();
            Archive();
        }

        public void UpsertPack(string name, List<Mod> allMods)
        {
            var active = allMods.Where(m => m.IsActive).Select(m => m.Id).ToList();
            Packs[name] = active;
            Archive();
        }

        public void DeletePack(string name)
        {
            Packs.Remove(name);
            Archive();
        }

        public string ExportPortable(string name, List<Mod> allMods)
        {
            var portableMods = allMods.Where(m => Packs[name].Contains(m.Id)).ToList();
            var portableVault = PortableVault.Build(portableMods, name);
            return Serialize(portableVault, SeriOptions);
        }

        public void ImportPortable(PortableVault portable)
        {
            var ids = portable.Mods.Select(m => m.Id).ToList();
            Packs[portable.Name] = ids;
            Archive();
        }
    }
}
