using Greed.Models.Online;
using System;

namespace Greed.Models.ListItem
{
    public class CatalogListItem
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string Author { get; set; }

        public string Version { get; set; }

        public string GreedVersion { get; set; }

        public string SinsVersion { get; set; }

        public string LastUpdated { get; set; }

        public string IsInstalled { get; set; }

        public CatalogListItem(OnlineMod m)
        {
            Name = m.Name;
            Id = m.Id ?? m.Name;
            Author = m.Author;
            Version = m.Latest.ToString();
            GreedVersion = m.Live.GreedVersion.ToString();
            SinsVersion = m.Live.SinsVersion.ToString();
            LastUpdated = !string.IsNullOrEmpty(m.Live.DateAdded)
                ? m.Live.DateAdded
                : DateTime.Today.ToString();
            IsInstalled = ModManager.IsModInstalled(Id) ? "✓" : " ";
        }
    }
}
