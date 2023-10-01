using Greed.Models.Json;
using System;
using System.IO;

namespace Greed.Models.ListItem
{
    public class SourceListItem
    {
        public string DeltaSymbol { get; set; }

        public string Folder { get; set; }

        public string Name { get; set; }

        public SourceListItem(JsonSource s, MainWindow window)
        {
            DeltaSymbol = "+";
            if (File.Exists(s.GoldPath))
            {
                try
                {
                    var goldStr = new JsonSource(s.GoldPath).Minify();
                    var modStr = s.Minify();
                    DeltaSymbol = modStr == goldStr ? "" : Constants.UNI_DELTA;
                }
                catch (Exception ex)
                {
                    window.CriticalAlertPopup("JSON Error: " + s.GoldPath, ex);
                }
            }
            Folder = s.Folder;
            Name = s.Mergename;
        }
    }
}
