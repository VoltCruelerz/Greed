using Greed.Models.Online;
using System.Linq;

namespace Greed.Models.ListItem
{
    public class ModListItem
    {
        public string Id { get; set; }

        public string Active { get; set; }

        public string Name { get; set; }

        public string Version { get; set; }

        public string Latest { get; set; }

        public string GreedVersion { get; set; }

        public string SinsVersion { get; set; }

        public bool IsLegal { get; set; }

        public ModListItem(Mod mod, MainWindow window, OnlineCatalog catalog)
        {
            Id = mod.Id;
            Active = mod.IsActive ? "✓" : " ";
            Name = mod.Meta.Name;
            Version = mod.Meta.Version.ToString();
            Latest = Version;
            GreedVersion = mod.Meta.GreedVersion.ToString();
            SinsVersion = mod.Meta.SinsVersion.ToString();

            // Handle violations
            var versionViolation = mod.Meta.IsLegalVersion();
            if (versionViolation.Contains("Mod"))
            {
                Version = "⚠ " + Version;
            }
            if (versionViolation.Contains("Greed"))
            {
                GreedVersion = "⚠ " + GreedVersion;
            }
            if (versionViolation.Contains("Sins"))
            {
                SinsVersion = "⚠ " + SinsVersion;
            }

            if (versionViolation.Any())
            {
                window.PrintAsync("WARNING - Incompatible Version: " + mod.Meta.Name + "\r\n- " + string.Join("\r\n- ", versionViolation));
            }

            // Handle outdated
            var onlineMod = catalog.Mods.FirstOrDefault(m => m.Id == mod.Id);
            if (onlineMod != null)
            {
                Latest = onlineMod.Latest.ToString();
                if (mod.Meta.Version.CompareTo(onlineMod.Latest) < 0)
                {
                    Version = "[⭳] " + Version;
                }
            }
        }
    }
}
