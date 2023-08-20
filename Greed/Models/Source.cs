using System.Configuration;

namespace Greed.Models
{
    public class Source
    {
        public string SourcePath { get; set; }
        public string GreedPath { get; set; }
        public string GoldPath { get; set; }
        public string Mod { get; set; }
        public string Folder { get; set; }
        public string Filename { get; set; }

        public Source(string sourcePath)
        {
            SourcePath = sourcePath;

            var folders = SourcePath.Split('\\');
            Mod = folders[^3];
            Folder = folders[^2];
            Filename = folders[^1];

            GoldPath = $"{ConfigurationManager.AppSettings["sinsDir"]!}\\{Folder}\\{Filename}";
            GreedPath = $"{ConfigurationManager.AppSettings["modDir"]!}\\greed\\{Folder}\\{Filename}";
        }

        public override string ToString()
        {
            return Filename;
        }

        public virtual Source Merge(Source source)
        {
            this.SourcePath = source.SourcePath;
            return this;
        }
    }
}
