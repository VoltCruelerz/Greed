using Newtonsoft.Json;
using System;

namespace Greed.Models
{
    public class Dependency
    {
        [JsonRequired]
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; } = string.Empty;

        [JsonRequired]
        [JsonProperty(PropertyName = "version")]
        public Version Version { get; set; } = new Version("0.0.0");

        public override string ToString()
        {
            return Id + " v" + Version.ToString();
        }

        /// <summary>
        /// Synchronously checks if the dependency is outdated or missing.
        /// </summary>
        /// <param name="desiredVersion"></param>
        /// <returns></returns>
        public bool IsOutdatedOrMissing()
        {
            if (!ModManager.IsModInstalled(Id))
            {
                return true;
            }

            var installed = LocalInstall.Load(Id);
            if (installed.GetVersion().CompareTo(Version) < 0)
            {
                return true;
            }
            return false;
        }
    }
}
