using Greed.Models.Metadata;
using System;

namespace Greed.Models.ListItem
{
    public class OnlineListItem
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string Author { get; set; }

        public string Version { get; set; }

        public string GreedVersion { get; set; }

        public string SinsVersion { get; set; }

        public string LastUpdated { get; set; }

        public OnlineListItem(OnlineMetadata m)
        {
            Name = m.Name;
            Id = m.Id ?? m.Name;
            Author = m.Author;
            Version = m.Version.ToString();
            GreedVersion = m.GreedVersion.ToString();
            SinsVersion = m.SinsVersion.ToString();
            LastUpdated = !string.IsNullOrEmpty(m.LastUpdated) ? m.LastUpdated : DateTime.Today.ToString();
        }
    }
}
