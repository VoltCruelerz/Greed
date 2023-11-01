using Greed.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Greed.Models.Config
{
    public class GlobalBool : GlobalSetting
    {
        public enum BoolType
        {
            ALL_RESEARCH,
            INFINITE_MONEY
        }

        [Newtonsoft.Json.JsonRequired]
        [JsonProperty(PropertyName = "type")]
        public BoolType Type { get; set; }

        [JsonProperty(PropertyName = "value")]
        public bool Value { get; set; } = false;

        public override void Init() { }

        public override void Exec()
        {
            if (!HasChanged()) return;

            try
            {
                _ = Type switch
                {
                    BoolType.ALL_RESEARCH => HandleAllResearch(),
                    BoolType.INFINITE_MONEY => HandleInfiniteFunds(),
                    _ => throw new InvalidOperationException("Unknown type: " + Type.ToString())
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to export global {Name}", ex);
            }
        }

        public override bool HasChanged()
        {
            return Value;
        }

        public bool HandleAllResearch()
        {
            var files = GetGlobalFiles("entities", ".player");
            var greedSettingFolder = Path.Combine(Settings.GetModDir(), "greed", "entities");

            // These are techs that can't be set to enabled right out of the gate.
            HashSet<string> badTechs = new List<string>()
            {
                "trader_rebel_pirate_cease_fire",
                "trader_rebel_pirate_share_vision"
            }.ToHashSet();

            try
            {
                foreach (var f in files)
                {
                    bool changed = false;
                    // Read the existing player file
                    var player = JObject.Parse(File.ReadAllText(f));

                    // Update the player file
                    var research = player["research"];
                    if (research != null)
                    {
                        var subjects = JArray.Parse(research["research_subjects"]?.ToString() ?? "[]")
                            .Where(s => !badTechs.Contains(s.ToString()))
                            .ToArray();
                        var arr = JArray.FromObject(subjects);
                        research["starting_research_subjects"] = arr;
                        changed = true;
                    }

                    // Write the update
                    if (changed) File.WriteAllText(Path.Combine(greedSettingFolder, Path.GetFileName(f)), player.ToString());
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to export global {Name}", ex);
            }
            return true;
        }

        public bool HandleInfiniteFunds()
        {
            var files = GetGlobalFiles("entities", ".player");
            var greedSettingFolder = Path.Combine(Settings.GetModDir(), "greed", "entities");

            var dsa = JObject.Parse("""
                {
                    "credits": 999999999,
                    "crystal": 999999999,
                    "metal": 999999999
                }
            """);

            var dse = JArray.Parse("""
                [
                    {
                        "exotic_type": "offense",
                        "count": 9999999
                    },
                    {
                        "exotic_type": "utility",
                        "count": 9999999
                    },
                    {
                        "exotic_type": "defense",
                        "count": 9999999
                    },
                    {
                        "exotic_type": "economic",
                        "count": 9999999
                    },
                    {
                        "exotic_type": "ultimate",
                        "count": 9999999
                    }
                ]
            """);
            try
            {
                foreach (var f in files)
                {
                    // Read the existing player file
                    var player = JObject.Parse(File.ReadAllText(f));

                    player["default_starting_assets"] = dsa;
                    player["default_starting_exotics"] = dse;

                    // Write the update
                    File.WriteAllText(Path.Combine(greedSettingFolder, Path.GetFileName(f)), player.ToString());
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to export global {Name}", ex);
            }
            return true;
        }
    }
}
