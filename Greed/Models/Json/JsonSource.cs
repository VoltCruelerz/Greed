using Greed.Diff;
using Greed.Extensions;
using JsonDiffer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.IO;
using System.Text;

namespace Greed.Models.Json
{
    public class JsonSource : Source
    {
        private static readonly string MergeReplaceSuffix = ".gmr";
        private static readonly string MergeUnionSuffix = ".gmu";
        private static readonly string MergeConcatSuffix = ".gmc";
        public SourceType Type { get; set; }
        public string Mergename { get; set; }
        public string Json { get; set; }

        public JsonSource(string sourcePath) : base(sourcePath)
        {
            Mergename = Filename;
            Type = SourceType.Overwrite;

            if (Mergename.EndsWith(MergeReplaceSuffix))
            {
                Mergename = Mergename[0..(Filename.Length - MergeReplaceSuffix.Length)];
                Type = SourceType.MergeReplace;
            }
            else if (Mergename.EndsWith(MergeUnionSuffix))
            {
                Mergename = Mergename[0..(Filename.Length - MergeUnionSuffix.Length)];
                Type = SourceType.MergeUnion;
            }
            else if (Mergename.EndsWith(MergeConcatSuffix))
            {
                Mergename = Mergename[0..(Filename.Length - MergeConcatSuffix.Length)];
                Type = SourceType.MergeConcat;
            }
            ValidateFileExtension();


            GoldPath = $"{ConfigurationManager.AppSettings["sinsDir"]!}\\{Folder}\\{Mergename}";
            GreedPath = $"{ConfigurationManager.AppSettings["modDir"]!}\\greed\\{Folder}\\{Mergename}";

            Json = ReadJsonWithComments(SourcePath);
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
            return new DiffResult(Gold.Json, Greedy.Json, diff);
        }

        public DiffResult DiffFromGold()
        {
            if (File.Exists(GoldPath))
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

        private void ValidateFileExtension()
        {
            var isBespoke = Mergename.EndsWith(".localized_text") || Mergename.EndsWith(".entity_manifest");

            if (isBespoke && Type != SourceType.Overwrite)
            {
                throw new InvalidOperationException("You cannot use the greedy file extensions on " + Mergename);
            }
        }

        /// <summary>
        /// Greed supports source files with C-style comments.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string ReadJsonWithComments(string path)
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
    }
}
