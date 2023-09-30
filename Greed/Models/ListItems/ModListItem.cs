using Greed.Extensions;
using Greed.Models.Metadata;
using Greed.Models.Online;
using System;
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
            Active = mod.IsActive ? Constants.UNI_CHECK : " ";
            Name = mod.Meta.Name;
            Version = mod.Meta.Version.ToString();
            Latest = Version;
            GreedVersion = mod.Meta.GreedVersion.ToString();
            SinsVersion = mod.Meta.SinsVersion.ToString();

            // Handle violations
            var versionViolations = mod.Meta.IsLegalVersion();
            if (versionViolations.Contains(ViolationCauseEnum.ModGreedTooOld) || versionViolations.Contains(ViolationCauseEnum.ModSinsTooOld))
            {
                Version = Constants.UNI_WARN + " " + Version;
            }
            if (versionViolations.Contains(ViolationCauseEnum.ModGreedTooOld) || versionViolations.Contains(ViolationCauseEnum.LiveGreedTooOld))
            {
                GreedVersion = Constants.UNI_WARN + " " + GreedVersion;
            }
            if (versionViolations.Contains(ViolationCauseEnum.ModSinsTooOld) || versionViolations.Contains(ViolationCauseEnum.LiveSinsTooOld))
            {
                SinsVersion = Constants.UNI_WARN + " " + SinsVersion;
            }

            if (versionViolations.Any())
            {
                _ = window.PrintAsync("WARNING - Incompatible Version: " + mod.Meta.Name + Environment.NewLine + versionViolations.Select(v => v.GetDescription()).ToBulletedList());
            }

            // Handle outdated
            var onlineMod = catalog.Mods.FirstOrDefault(m => m.Id == mod.Id);
            if (onlineMod != null)
            {
                Latest = onlineMod.Latest.ToString();
                if (mod.Meta.Version.CompareTo(onlineMod.Latest) < 0)
                {
                    Version = Constants.UNI_READY_FOR_UPDATE + " " + Version;
                }
            }
        }
    }
}
