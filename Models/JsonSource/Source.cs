using Greed.Models.JsonSource.Entities;
using System.Configuration;
using System.IO;

namespace Greed.Models.JsonSource
{
    public class Source
    {
        public string SourcePath { get; set; }
        public string GreedPath { get; set; }
        public string GoldPath { get; set; }
        public string Mod { get; set; }
        public string Folder { get; set; }
        public string Filename { get; set; }
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
            GoldPath = $"{ConfigurationManager.AppSettings["sinsDir"]!}\\{Folder}\\{Filename}";
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
    }
}
