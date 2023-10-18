using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Greed.Models.Mutations.Paths;
using Greed.Models.Mutations.Variables;
using Greed.Utils;
using Newtonsoft.Json.Linq;

namespace Greed.Models
{
    public class GlobalScalar
    {
        public enum GlobalType
        {
            INT,
            DOUBLE
        }

        public string Folder { get; set; }
        public string Name { get; set; }
        public GlobalType Type { get; set; }
        public double Multiplier { get; set; }
        public double Default { get; set; }
        public string Extension { get; set; }
        public List<ActionPath> NodePath { get; set; }

        public GlobalScalar(string folder, string name, GlobalType type, double mult, double def, string extension, string nodePath)
        {
            Folder = folder;
            Name = name;
            Type = type;
            Multiplier = mult;
            Default = def;
            Extension = extension;
            NodePath = ActionPath.Build(nodePath);
        }

        public static List<GlobalScalar> GetGlobals()
        {
            return new List<GlobalScalar>()
            {
                new GlobalScalar("entities", Settings.Supply, GlobalType.INT, Settings.GetSliderMult(Settings.Supply), 1.0, ".player", "max_supply.levels[i].max_supply"),
                new GlobalScalar("entities", Settings.NumTitans, GlobalType.INT, Settings.GetSliderMult(Settings.NumTitans), 1.0, ".player", "unit_limits.global[i].unit_limit"),

                new GlobalScalar("entities", Settings.TacticalSlots, GlobalType.INT, Settings.GetSliderMult(Settings.TacticalSlots), 1.0, ".player", "planet_types[i].tracks.defense[j].max_military_structure_slots"),
                new GlobalScalar("entities", Settings.LogisticalSlots, GlobalType.INT, Settings.GetSliderMult(Settings.LogisticalSlots), 1.0, ".player", "planet_types[i].tracks.logistics[j].max_civilian_structure_slots"),

                new GlobalScalar("entities", Settings.WeaponDamage, GlobalType.DOUBLE, Settings.GetSliderMult(Settings.WeaponDamage), 1.0, ".weapon", "damage"),
                new GlobalScalar("entities", Settings.BombingDamage, GlobalType.DOUBLE, Settings.GetSliderMult(Settings.BombingDamage), 1.0, ".weapon", "bombing_damage"),

                new GlobalScalar("entities", Settings.Antimatter + "_base_max", GlobalType.DOUBLE, Settings.GetSliderMult(Settings.Antimatter), 1.0, ".unit", "antimatter.max_antimatter"),
                new GlobalScalar("entities", Settings.Antimatter + "_base_regen", GlobalType.DOUBLE, Settings.GetSliderMult(Settings.Antimatter), 1.0, ".unit", "antimatter.antimatter_restore_rate"),
                new GlobalScalar("entities", Settings.Antimatter + "_level_max", GlobalType.DOUBLE, Settings.GetSliderMult(Settings.Antimatter), 1.0, ".unit", "levels.levels[i].unit_modifiers.additive_values.max_antimatter"),
                new GlobalScalar("entities", Settings.Antimatter + "_level_regen", GlobalType.DOUBLE, Settings.GetSliderMult(Settings.Antimatter), 1.0, ".unit", "levels.levels[i].unit_modifiers.additive_values.antimatter_restore_rate"),

                new GlobalScalar("entities", Settings.ExperienceForLevel, GlobalType.DOUBLE, Settings.GetSliderMult(Settings.ExperienceForLevel), 1.0, ".unit", "levels.levels[i].experience_to_next_level"),

                new GlobalScalar("entities", Settings.UnitCost + "_credits", GlobalType.DOUBLE, Settings.GetSliderMult(Settings.UnitCost), 1.0, ".unit", "build.price.credits"),
                new GlobalScalar("entities", Settings.UnitCost + "_metal", GlobalType.DOUBLE, Settings.GetSliderMult(Settings.UnitCost), 1.0, ".unit", "build.price.metal"),
                new GlobalScalar("entities", Settings.UnitCost + "_crystal", GlobalType.DOUBLE, Settings.GetSliderMult(Settings.UnitCost), 1.0, ".unit", "build.price.crystal"),

                new GlobalScalar("entities", Settings.GravityWellSize, GlobalType.DOUBLE, Settings.GetSliderMult(Settings.GravityWellSize), 1.0, ".unit", "gravity_well_fixture.inner_move_distance")
            };
        }

        public void Exec()
        {
            if (Multiplier == Default) return;

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

                    var madeAChange = false;
                    NodePath[0].DoWork(obj, NodePath, 0, new(), (JToken? token, Dictionary<string, Variable> variables, int depth) =>
                    {
                        if (token == null) return;
                        if (token is JObject obj)
                        {
                            var fp = (FieldPath)NodePath[^1];
                            if (obj[fp.Name] == null) return;
                            if (Type == GlobalType.DOUBLE)
                            {
                                obj[fp.Name] = obj[fp.Name]!.Value<double>() * Multiplier;
                                madeAChange = true;
                            }
                            else if (Type == GlobalType.INT)
                            {
                                obj[fp.Name] = (int)(obj[fp.Name]!.Value<int>() * Multiplier);
                                madeAChange = true;
                            }
                        }
                        else if (token is JArray arr)
                        {
                            var i = arr.IndexOf(token);
                            if (Type == GlobalType.DOUBLE)
                            {
                                arr[i] = token.Value<double>() * Multiplier;
                                madeAChange = true;
                            }
                            else if (Type == GlobalType.INT)
                            {
                                arr[i] = (int)(token.Value<int>() * Multiplier);
                                madeAChange = true;
                            }
                        }
                        if (!madeAChange)
                        {
                            throw new Exception($"Failed to find work for global {Name}");
                        }
                    });

                    if (madeAChange) File.WriteAllText(Path.Combine(greedSettingFolder, Path.GetFileName(f)), obj.ToString());
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to export global {Name}", ex);
            }
        }
    }
}
