using Greed.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Greed.Models.Config
{
    public abstract class GlobalSetting
    {
        [Newtonsoft.Json.JsonRequired]
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; } = string.Empty;

        public abstract bool HasChanged();
        public abstract void Exec();
        public abstract void Init();

        protected List<string> GetGlobalFiles(string folder, string extension)
        {
            try
            {
                // Load existing files
                var greedSettingFolder = Path.Combine(Settings.GetModDir(), "greed", folder);
                if (!Directory.Exists(greedSettingFolder)) Directory.CreateDirectory(greedSettingFolder);
                var greedFile = Directory.GetFiles(greedSettingFolder)
                .Where(f => Path.GetExtension(f) == extension)
                    .ToList();
                var goldFiles = Directory.GetFiles(Path.Combine(Settings.GetSinsDir(), folder))
                    .Where(f => Path.GetExtension(f) == extension)
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
                return fileDict.Values.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to export global {Name}", ex);
            }
        }
    }
}
