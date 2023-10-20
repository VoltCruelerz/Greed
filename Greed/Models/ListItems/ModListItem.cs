using Greed.Controls;
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

        public string Displayname { get; set; }

        public string Version { get; set; }

        public string Latest { get; set; }

        public string GreedVersion { get; set; }

        public string SinsVersion { get; set; }

        public bool IsLegal { get; set; }

        public bool IsEven { get; set; }

        public bool IsSelected { get; set; }

        public ModListItem(Mod mod, OnlineCatalog catalog, bool isEven, bool isSelected)
        {
            Id = mod.Id;
            Active = mod.IsActive ? Utils.Constants.UNI_CHECK : " ";
            Displayname = mod.Meta.Name;
            Version = mod.Meta.Version.ToString();
            Latest = Version;
            GreedVersion = mod.Meta.GreedVersion.ToString();
            SinsVersion = mod.Meta.SinsVersion.ToString();
            IsEven = isEven;
            IsSelected = isSelected;
            IsLegal = true;


            // Handle violations
            var versionViolations = mod.Meta.IsLegalVersion();
            if (versionViolations.Contains(ViolationCauseEnum.ModGreedTooOld) || versionViolations.Contains(ViolationCauseEnum.ModSinsTooOld))
            {
                Version = Utils.Constants.UNI_WARN + " " + Version;
            }
            if (versionViolations.Contains(ViolationCauseEnum.ModGreedTooOld) || versionViolations.Contains(ViolationCauseEnum.LiveGreedTooOld))
            {
                GreedVersion = Utils.Constants.UNI_WARN + " " + GreedVersion;
            }
            if (versionViolations.Contains(ViolationCauseEnum.ModSinsTooOld) || versionViolations.Contains(ViolationCauseEnum.LiveSinsTooOld))
            {
                SinsVersion = Utils.Constants.UNI_WARN + " " + SinsVersion;
            }

            if (versionViolations.Any())
            {
                Log.Warn("Incompatible Version: " + mod.Meta.Name + Environment.NewLine + versionViolations.Select(v => v.GetDescription()).ToBulletedList());
                IsLegal = false;
            }

            // Handle outdated
            var onlineMod = catalog.Mods.FirstOrDefault(m => m.Id == mod.Id);
            if (onlineMod != null)
            {
                Latest = onlineMod.Latest.ToString();
                if (mod.Meta.Version.CompareTo(onlineMod.Latest) < 0)
                {
                    Version = Utils.Constants.UNI_READY_FOR_UPDATE + " " + Version;
                }
            }
        }

        public override string ToString()
        {
            return Displayname + " v" + Version;
        }
    }
}
