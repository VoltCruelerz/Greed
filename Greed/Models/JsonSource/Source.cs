using Newtonsoft.Json.Linq;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System;
using Newtonsoft.Json;
using Greed.Extensions;
using Greed.Models.JsonSource.Text;
using Greed.Models.JsonSource.Entities;
using JsonDiffer;
using System.Text;

namespace Greed.Models.JsonSource
{
    public class Source
    {
        private static readonly string MergeReplaceSuffix = ".gmr";
        private static readonly string MergeUnionSuffix = ".gmu";
        private static readonly string MergeConcatSuffix = ".gmc";

        public string SourcePath { get; set; }
        public string GreedPath { get; set; }
        public string GoldPath { get; set; }
        public string Mod { get; set; }
        public string Folder { get; set; }
        public string Filename { get; set; }
        public SourceType Type { get; set; }
        public string Mergename { get; set; }
        public string Json { get; set; }

        public Source(string sourcePath)
        {
            SourcePath = sourcePath;

            var folders = SourcePath.Split('\\');
            Mod = folders[^3];
            Folder = folders[^2];
            Filename = folders[^1];
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

        public virtual Source Merge(Source other)
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

        public virtual Source Clone()
        {
            return new Source(SourcePath);
        }

        public virtual string Diff(Source other)
        {
            var j1 = JToken.Parse(Json);
            var j2 = JToken.Parse(other.Json);

            var diff = JsonConvert.SerializeObject(JsonDifferentiator.Differentiate(j1, j2), Formatting.Indented);
            return diff;
        }

        public string DiffFromGold()
        {
            return Filename + "\n" + Diff(new Source(GoldPath));
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
                if (cur == '/' &&  next == '/')
                {
                    var found = false;
                    for (var j = i + 2 ; j < str.Length; j++)
                    {
                        var hypo = str[j];
                        if (hypo == '\r' ||  hypo == '\n')
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
