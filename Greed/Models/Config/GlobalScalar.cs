using Greed.Models.Mutations.Paths;
using Greed.Models.Mutations.Variables;
using Greed.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;

namespace Greed.Models.Config
{
    public class GlobalScalar
    {
        public enum GlobalType
        {
            INT,
            DOUBLE
        }

        [Newtonsoft.Json.JsonRequired]
        [JsonProperty(PropertyName = "folder")]
        public string Folder { get; set; } = string.Empty;

        [Newtonsoft.Json.JsonRequired]
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; } = string.Empty;

        [Newtonsoft.Json.JsonRequired]
        [JsonProperty(PropertyName = "type")]
        public GlobalType Type { get; set; }

        [JsonProperty(PropertyName = "value")]
        public double Value { get; set; } = 1.0;

        [Newtonsoft.Json.JsonRequired]
        [JsonProperty(PropertyName = "extension")]
        public string Extension { get; set; } = string.Empty;

        [Newtonsoft.Json.JsonRequired]
        [JsonProperty(PropertyName = "locations")]
        public List<EditLocation> Locations { get; set; } = new();

        public void Init()
        {
            Locations.ForEach(loc => loc.Init());
        }

        public void Exec()
        {
            if (!HasChanged()) return;

            try
            {
                // Load existing files
                var greedSettingFolder = Path.Combine(Settings.GetModDir(), "greed", Folder);
                if (!Directory.Exists(greedSettingFolder)) Directory.CreateDirectory(greedSettingFolder);
                var greedFile = Directory.GetFiles(greedSettingFolder)
                    .Where(f => Path.GetExtension(f) == Extension)
                    .ToList();
                var goldFiles = Directory.GetFiles(Path.Combine(Settings.GetSinsDir(), Folder))
                    .Where(f => Path.GetExtension(f) == Extension)
                    .ToList();

                // Use greed if available, default to gold.
                var fileDict = greedFile.ToDictionary(f => Path.GetFileName(f));
                goldFiles.ForEach(f =>
                {
                    var greedName = Path.GetFileName(f);
                    if (!fileDict.ContainsKey(greedName))
                    {
                        fileDict.Add(greedName, f);
                    }
                });
                var files = fileDict.Values.ToList();

                // Apply the changes
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

        public bool HasChanged()
        {
            return Value != 1.0;
        }
    }
}
