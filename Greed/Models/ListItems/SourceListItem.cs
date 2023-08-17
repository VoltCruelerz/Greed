using Greed.Models.JsonSource;
using System;
using System.IO;

namespace Greed.Models.ListItem
{
    public class SourceListItem
    {
        public string DeltaSymbol { get; set; }

        public string Folder { get; set; }

        public string Name { get; set; }

        public SourceListItem(Source s)
        {
            DeltaSymbol = "+";
            if (File.Exists(s.GoldPath))
            {
                var goldStr = new Source(s.GoldPath).Minify();
                var modStr = s.Minify();
                DeltaSymbol = modStr == goldStr ? "" : "Δ";
            }
            Folder = s.Folder;
            Name = s.Mergename;
        }
    }
}
