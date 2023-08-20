using Greed.Models.Json;
using System.IO;

namespace Greed.Models.ListItem
{
    public class SourceListItem
    {
        public string DeltaSymbol { get; set; }

        public string Folder { get; set; }

        public string Name { get; set; }

        public SourceListItem(JsonSource s)
        {
            DeltaSymbol = "+";
            if (File.Exists(s.GoldPath))
            {
                var goldStr = new JsonSource(s.GoldPath).Minify();
                var modStr = s.Minify();
                DeltaSymbol = modStr == goldStr ? "" : "Δ";
            }
            Folder = s.Folder;
            Name = s.Mergename;
        }
    }
}
