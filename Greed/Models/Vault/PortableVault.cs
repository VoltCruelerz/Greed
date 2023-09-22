using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using static System.Text.Json.JsonSerializer;

namespace Greed.Models.Vault
{
    public class PortableVault
    {
        [JsonRequired]
        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; } = string.Empty;


        [JsonRequired]
        [JsonProperty(PropertyName = "Mods")]
        public List<Dependency> Mods { get; set; } = new();

        public static PortableVault Build(List<Mod> mods, string vaultName)
        {
            return new PortableVault
            {
                Name = vaultName,
                Mods = mods.Select(m => new Dependency
                {
                    Id = m.Id,
                    Version = m.Meta.GetVersion()
                }).ToList()
            };
        }

        public static PortableVault Load(string json)
        {
            return Deserialize<PortableVault>(json)!;
        }
    }
}
