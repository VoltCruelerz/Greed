using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Greed.Models
{
    public class Metadata
    {
        [JsonRequired]
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; } = string.Empty;

        [JsonRequired]
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; } = string.Empty;

        [JsonRequired]
        [JsonProperty(PropertyName = "author")]
        public string Author { get; set; } = string.Empty;

        [JsonRequired]
        [JsonProperty(PropertyName = "version")]
        public Version Version { get; set; } = new Version("0.0.0");

        [JsonRequired]
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; } = string.Empty;

        [JsonRequired]
        [JsonProperty(PropertyName = "greedVersion")]
        public Version GreedVersion { get; set; } = new Version("0.0.0");

        [JsonRequired]
        [JsonProperty(PropertyName = "sinsVersion")]
        public Version SinsVersion { get; set; } = new Version("0.0.0.0");

        [JsonRequired]
        [JsonProperty(PropertyName = "dependencies")]
        public List<Dependency> Dependencies { get; set; } = new List<Dependency>();

        [JsonProperty(PropertyName = "conflicts")]
        public List<string> Conflicts { get; set; } = new List<string>();

        /// <summary>
        /// Returns which file version is mismatched.
        /// </summary>
        /// <returns>"Greed" and/or/neither "Sins", as invalid</returns>
        public List<string> IsLegalVersion()
        {
            var sinsDir = ConfigurationManager.AppSettings["sinsDir"]!;
            var sinsVersion = new Version(FileVersionInfo.GetVersionInfo(sinsDir + "\\sins2.exe").FileVersion!);
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
                    violations.Add($"- missing {d.Id} v {d.Version}");
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
    }
}
