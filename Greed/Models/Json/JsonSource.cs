using Greed.Controls.Diff;
using Greed.Extensions;
using Greed.Models.Vault;
using JsonDiffer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Text.Json;
using Greed.Exceptions;
using static System.Text.Json.JsonSerializer;
using System.Linq;
using System.Diagnostics;

namespace Greed.Models.Json
{
    public class JsonSource : Source
    {
        private const string MergeReplaceSuffix = "gmr";
        private const string MergeUnionSuffix = "gmu";
        private const string MergeConcatSuffix = "gmc";

        private static readonly List<string> NoGoldFiles = new()
        {
            "*.entity_manifest",
            "unit_tag.uniforms"
        };

        public SourceType Type { get; set; }
        public string Mergename { get; set; }
        public string Json { get; set; }
        public SourceGreedRules? GreedRules { get; set; }

        /// <summary>
        /// It's possible the parent was defined by another mod.
        /// </summary>
        public string? ParentGreedPath { get; set; }

        public JsonSource(string sourcePath) : base(sourcePath)
        {
            Mergename = Filename;
            Type = GetSourceType(Path.GetExtension(Filename));

            if (Type != SourceType.Overwrite)
            {
                Mergename = Mergename[0..(Filename.Length - 4)];
            }
            GoldPath = $"{ConfigurationManager.AppSettings["sinsDir"]!}\\{Folder}\\{Mergename}";
            GreedPath = $"{ConfigurationManager.AppSettings["modDir"]!}\\greed\\{Folder}\\{Mergename}";

            Json = ReadJsonWithComments(SourcePath);

            // Handle the greed rules
            try
            {
                var jObjWithGreed = JObject.Parse(Json);
                if (jObjWithGreed["greed"] != null)
                {
                    // Parse the config
                    var configStr = jObjWithGreed["greed"]!.ToString();
                    GreedRules = JsonConvert.DeserializeObject<SourceGreedRules>(configStr)!;

                    // Clean up the JSON to remove the greed config.
                    jObjWithGreed.Remove("greed");
                    Json = jObjWithGreed.ToString();

                    // Handle the rules
                    if (GreedRules.Parent != null)
                    {
                        /*
                         * When loading something with a parent...
                         * 1. Try loading self from greed
                         * 2. Try loading parent from greed
                         * 3. Try loading parent from gold
                         */
                        var extension = Path.GetExtension(Mergename);
                        var parentFilename = GreedRules.Parent + extension;
                        GoldPath = $"{ConfigurationManager.AppSettings["sinsDir"]!}\\{Folder}\\{parentFilename}";
                        ParentGreedPath = $"{ConfigurationManager.AppSettings["modDir"]!}\\greed\\{Folder}\\{parentFilename}";
                    }
                    if (GreedRules.Alias != null)
                    {
                        Mergename = GreedRules.Alias;
                        GoldPath = $"{ConfigurationManager.AppSettings["sinsDir"]!}\\{Folder}\\{Mergename}";
                        GreedPath = $"{ConfigurationManager.AppSettings["modDir"]!}\\greed\\{Folder}\\{Mergename}";
                    }
                    if (GreedRules.MergeMode != null)
                    {
                        Type = GetSourceType(GreedRules.MergeMode);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ModExportException("Failed to parse JSON for " + sourcePath, ex);
            }
        }

        private SourceType GetSourceType(string type)
        {
            type = type.Replace(".", "");
            return type switch
            {
                MergeReplaceSuffix => SourceType.MergeReplace,
                MergeUnionSuffix => SourceType.MergeUnion,
                MergeConcatSuffix => SourceType.MergeConcat,
                _ => SourceType.Overwrite,
            };
        }

        public virtual JsonSource Merge(JsonSource other)
        {
            var a = JObject.Parse(Json);
            var b = JObject.Parse(other.Json);
            MergeArrayHandling? handler = other.Type switch
            {
                SourceType.MergeReplace => (MergeArrayHandling?)MergeArrayHandling.Replace,
                SourceType.MergeConcat => (MergeArrayHandling?)MergeArrayHandling.Concat,
                SourceType.MergeUnion => (MergeArrayHandling?)MergeArrayHandling.Union,
                SourceType.Overwrite => null,
                _ => throw new NotImplementedException("Unrecognized type for other: " + other.Type.ToString()),
            };

            if (handler == null)
            {
                Json = other.Json;
                a = JObject.Parse(Json);
            }
            else
            {
                a.Merge(b, new JsonMergeSettings
                {
                    MergeArrayHandling = (MergeArrayHandling)handler,
                    MergeNullValueHandling = MergeNullValueHandling.Merge,
                });
            }

            // We shouldn't have to do this step, but Nullable doesn't play nice with Newtonsoft yet.
            PurgeNulls(a);

            Json = a.ToString();
            return this;
        }

        public override string ToString()
        {
            return Json;
        }

        public virtual string Minify()
        {
            return Json.JsonMinify();
        }

        public virtual JsonSource Clone()
        {
            return new JsonSource(SourcePath);
        }

        public virtual DiffResult Diff(JsonSource Gold, JsonSource Greedy)
        {
            var j1 = JToken.Parse(Gold.Json);
            var j2 = JToken.Parse(Greedy.Json);
            var diffObj = JsonDifferentiator.Differentiate(j1, j2, OutputMode.Symbol, true);

            var diff = JsonConvert.SerializeObject(diffObj, Formatting.Indented);
            return new DiffResult(Gold.Json.JsonFormat(), Greedy.Json.JsonFormat(), diff);
        }

        public DiffResult DiffFromGold(List<Mod> active)
        {
            if (File.Exists(GreedPath))
            {
                var gold = new JsonSource(GoldPath);
                var greed = gold.Clone().Merge(this);
                greed.SourcePath = this.SourcePath;
                return Diff(gold, greed);
            }
            return new DiffResult("", Json, Json);
        }

        /// <summary>
        /// These two functions exist because .NET 7's null handling doesn't play nice with Newtonsoft yet. They rip out nulls.
        /// </summary>
        /// <param name="obj"></param>
        private void PurgeNulls(JObject obj)
        {
            var copy = JObject.FromObject(obj);
            foreach (var item in copy)
            {
                if (item.Value!.Type == JTokenType.Null)
                {
                    obj.Remove(item.Key);
                }
                else if (item.Value is JObject)
                {
                    var childObj = (JObject)obj[item.Key]!;
                    PurgeNulls(childObj);
                }
                else if (item.Value is JArray)
                {
                    var childArr = (JArray)obj[item.Key]!;
                    PurgeNulls(childArr);
                }
            }
        }
        private void PurgeNulls(JArray arr)
        {
            var copy = JArray.FromObject(arr);
            var i = 0;
            foreach (var element in copy)
            {
                var realElement = arr[i];
                if (element.Type == JTokenType.Null)
                {
                    arr[i].Remove();
                    continue;
                }
                else if (element is JObject)
                {
                    PurgeNulls((JObject)realElement);
                }
                else if (element is JArray)
                {
                    PurgeNulls((JArray)realElement);
                }
                i++;
            }
        }

        public bool RequiresGold()
        {
            for (int i = 0; i < NoGoldFiles.Count; i++)
            {
                var noGold = NoGoldFiles[i];
                if (noGold.StartsWith("*"))
                {
                    var realNoGold = noGold[1..];
                    if (Mergename.EndsWith(realNoGold))
                    {
                        return false;
                    }
                }
                else if (noGold == Mergename)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Greed supports source files with C-style comments.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ReadJsonWithComments(string path)
        {
            var str = File.ReadAllText(path);
            var sb = new StringBuilder();

            // All comment characters are length 2, so it's < -1
            for (var i = 0; i < str.Length - 1; i++)
            {
                var cur = str[i];
                var next = str[i + 1];

                // Single-line comment
                if (cur == '/' && next == '/')
                {
                    var found = false;
                    for (var j = i + 2; j < str.Length; j++)
                    {
                        var hypo = str[j];
                        if (hypo == '\r' || hypo == '\n')
                        {
                            // The loop's ++ will bump us back to j.
                            i = j - 1;
                            found = true;
                            break;
                        }
                    }

                    // If we never found it a line break, that means we hit the end of he file, so we're done.
                    if (!found)
                    {
                        return sb.ToString();
                    }
                }
                // Multi-line comment.
                else if (cur == '/' && next == '*')
                {
                    var found = false;
                    for (var j = i + 2; j < str.Length - 1; j++)
                    {
                        var hypoCur = str[j];
                        var hypoNext = str[j + 1];
                        if (hypoCur == '*' && hypoNext == '/')
                        {
                            i = j + 1;
                            found = true;
                            break;
                        }
                    }

                    // If we never found it, that means they didn't close the comment.
                    if (!found)
                    {
                        throw new InvalidOperationException("Invalid jsonc file: " + path);
                    }
                }
                else
                {
                    sb.Append(cur);
                }
            }
            // Don't forget to add the last character.
            sb.Append(str[^1]);

            return sb.ToString();
        }

        /// <summary>
        /// Folder -> Mergename -> Export Order -> Filename
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(JsonSource other)
        {
            // Folder
            var diff = Folder.CompareTo(other.Folder);
            if (diff != 0) return diff;

            // Merge
            diff = Mergename.CompareTo(other.Mergename);
            if (diff != 0) return diff;

            // Export Order
            var order = GreedRules?.ExportOrder ?? 0;
            var otherOrder = other.GreedRules?.ExportOrder ?? 0;
            diff = order.CompareTo(otherOrder);
            if (diff != 0) return diff;

            // Filename
            return Filename.CompareTo(other.Filename);
        }
    }
}
