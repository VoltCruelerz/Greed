using Greed.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Greed.Models.Config
{
    public class GlobalScalar : GlobalSetting
    {
        public enum ScalarType
        {
            INT,
            DOUBLE
        }

        [Newtonsoft.Json.JsonRequired]
        [JsonProperty(PropertyName = "folder")]
        public string Folder { get; set; } = string.Empty;

        [Newtonsoft.Json.JsonRequired]
        [JsonProperty(PropertyName = "type")]
        public ScalarType Type { get; set; }

        [JsonProperty(PropertyName = "value")]
        public double Value { get; set; } = 1.0;

        [Newtonsoft.Json.JsonRequired]
        [JsonProperty(PropertyName = "extension")]
        public string Extension { get; set; } = string.Empty;

        [Newtonsoft.Json.JsonRequired]
        [JsonProperty(PropertyName = "locations")]
        public List<EditLocation> Locations { get; set; } = new();

        public override void Init()
        {
            Locations.ForEach(loc => loc.Init());
        }

        public override void Exec()
        {
            if (!HasChanged()) return;

            var files = GetGlobalFiles(Folder, Extension);

            try
            {
                // Apply the changes
                var greedSettingFolder = Path.Combine(Settings.GetModDir(), "greed", Folder);
                foreach (var f in files)
                {
                    var obj = JObject.Parse(File.ReadAllText(f));
                    var changes = Locations.Sum(loc => loc.Exec(obj, this));
                    if (changes > 0) File.WriteAllText(Path.Combine(greedSettingFolder, Path.GetFileName(f)), obj.ToString());
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to export global {Name}", ex);
            }
        }

        public override bool HasChanged()
        {
            return Value != 1.0;
        }
    }
}
