using Greed.Models.Metadata;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace Greed.Models
{
    public class LocalInstall : BasicMetadata
    {
        [JsonRequired]
        [JsonProperty(PropertyName = "version")]
        public Version Version { get; set; } = new Version("0.0.0");

        [JsonRequired]
        [JsonProperty(PropertyName = "greedVersion")]
        public Version GreedVersion { get; set; } = new Version("0.0.0");

        [JsonRequired]
        [JsonProperty(PropertyName = "sinsVersion")]
        public Version SinsVersion { get; set; } = new Version("0.0.0.0");

        [JsonRequired]
        [JsonProperty(PropertyName = "dependencies")]
        public List<Dependency> Dependencies { get; set; } = new List<Dependency>();

        [JsonRequired]
        [JsonProperty(PropertyName = "conflicts")]
        public List<string> Conflicts { get; set; } = new List<string>();

        public override Version GetVersion() { return Version; }

        public override Version GetGreedVersion() { return GreedVersion; }

        public override Version GetSinsVersion() { return SinsVersion; }

        public override List<Dependency> GetDependencies() { return Dependencies; }

        public override List<string> GetConflicts() { return Conflicts; }

        /// <summary>
        /// Returns which file version is mismatched.
        /// </summary>
        /// <returns>"Greed" and/or/neither "Sins", as invalid</returns>
        public List<string> IsLegalVersion()
        {
            var sinsDir = ConfigurationManager.AppSettings["sinsDir"]!;
            var sinsPath = sinsDir + "\\sins2.exe";
            var versionStr = File.Exists(sinsPath) ? FileVersionInfo.GetVersionInfo(sinsPath).FileVersion! : "0.0.0";
            var sinsVersion = new Version(versionStr);
            return IsLegalVersion(sinsVersion);
        }

        public List<string> IsLegalVersion(Version liveSinsVersion)
        {
            // This needs to be updated any time there's a breaking Greed change.
            var minimumCompatibleVersion = new Version("1.4.0");

            var violations = new List<string>();
            var liveVersion = Assembly.GetExecutingAssembly().GetName().Version!;
            if (liveVersion.CompareTo(GreedVersion) < 0)
            {
                Debug.WriteLine("Deprecated greed version.");
                violations.Add("A new version of Greed is required.");
            }
            else if (GreedVersion.CompareTo(minimumCompatibleVersion) < 0)
            {
                Debug.WriteLine("Greed no longer supports this version.");
                violations.Add("Mod must be updated. Contact developer if none is available.");
            }

            if (liveSinsVersion.CompareTo(SinsVersion) < 0)
            {
                Debug.WriteLine("Deprecated sins version.");
                violations.Add("This Sins version is no longer supported");
            }
            return violations;
        }

        /// <summary>
        /// Gets the lists of violations when attempting to activate this mod caused by dependencies missing, being outdated, or being inactive.
        /// </summary>
        /// <param name="allMods"></param>
        /// <returns>The list of violation strings (for a popup) and the list of offending mods.</returns>
        public (List<string>, List<Mod>) GetDependencyViolations(List<Mod> allMods)
        {
            var active = allMods.Where(m => m.IsActive);
            var violations = new List<string>();
            var inactiveDependencies = new List<Mod>();

            Dependencies.ForEach(d =>
            {
                var referencedMod = allMods.FirstOrDefault(a => a.Id == d.Id);
                if (referencedMod == null)
                {
                    violations.Add($"- missing {d.Id} v{d.Version}");
                }
                else
                {
                    if (referencedMod.Meta.Version.CompareTo(d.Version) < 0)
                    {
                        violations.Add($"- outdated {referencedMod.Meta.Name} v{referencedMod.Meta.Version} (needs at least {d.Version})");
                    }
                    else if (!referencedMod.IsActive)
                    {
                        violations.Add($"- required mod {referencedMod.Meta.Name} v{referencedMod.Meta.Version} is inactive");
                        inactiveDependencies.Add(referencedMod);
                    }
                }
            });

            return (violations, inactiveDependencies);
        }

        public List<Mod> GetDependencyMods(List<Mod> allMods)
        {
            return allMods.Where(m => Dependencies.Any(d => d.Id == m.Id)).ToList();
        }

        /// <summary>
        /// Synchronously loads a LocalInstall from the mods directory.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static LocalInstall Load(string id)
        {
            var modDir = ConfigurationManager.AppSettings["modDir"]!;
            var greedPath = modDir + "\\" + id + "\\greed.json";
            return JsonConvert.DeserializeObject<LocalInstall>(File.ReadAllText(greedPath))!;
        }
    }
}
