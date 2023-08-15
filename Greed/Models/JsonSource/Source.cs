using Greed.Models.JsonSource.Entities;
using Newtonsoft.Json.Linq;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System;
using Newtonsoft.Json;
using Greed.Extensions;

namespace Greed.Models.JsonSource
{
    public class Source
    {
        private static readonly string ConcatSuffix = "." + SourceType.Concat.ToString().ToLower();

        public string SourcePath { get; set; }
        public string GreedPath { get; set; }
        public string GoldPath { get; set; }
        public string Mod { get; set; }
        public string Folder { get; set; }
        public string Filename { get; set; }
        public SourceType Type { get; set; }
        public string Mergename { get; set; }
        public string Json { get; set; }
        public bool NeedsGold { get; set; }
        public bool NeedsMerge { get; set; }

        public Source(string sourcePath)
        {
            NeedsGold = false;
            NeedsGold = false;

            SourcePath = sourcePath;

            var folders = SourcePath.Split('\\');
            Mod = folders[^3];
            Folder = folders[^2];
            Filename = folders[^1];
            Mergename = Filename;
            Type = SourceType.Replace;

            if (Mergename.EndsWith(ConcatSuffix))
            {
                Mergename = Mergename[(Filename.Length - ConcatSuffix.Length)..];
                Type = SourceType.Concat;
            }


            GoldPath = $"{ConfigurationManager.AppSettings["sinsDir"]!}\\{Folder}\\{Mergename}";
            GreedPath = $"{ConfigurationManager.AppSettings["modDir"]!}\\greed\\{Folder}\\{Filename}";

            Json = File.ReadAllText(SourcePath);
        }

        public static Source BuildEntity(string path)
        {
            var folders = path.Split('\\');
            var filename = folders[^1];
            if (filename.EndsWith("entity_manifest"))
            {
                return new EntityManifest(path);
            }
            return new Entity(path);
        }

        public void Merge(Source other)
        {
            var a = JObject.Parse(Json);
            var b = JObject.Parse(other.Json);
            var handler = other.Type == SourceType.Replace
                ? MergeArrayHandling.Replace
                : MergeArrayHandling.Concat;
            a.Merge(b, new JsonMergeSettings
            {
                MergeArrayHandling = handler,
                MergeNullValueHandling = MergeNullValueHandling.Merge,
            });

            // We shouldn't have to do this step, but Nullable doesn't play nice with Newtonsoft yet.
            PurgeNulls(a);

            Json = a.ToString();
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

        public override string ToString()
        {
            return Json;
        }

        public string Minify()
        {
            return Json.JsonMinify();
        }

        public Source Clone()
        {
            return new Source(SourcePath);
        }
    }
}
