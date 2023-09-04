using System.Linq;

namespace Greed.Models.ListItem
{
    public class OnlineListItem
    {
        public string Name { get; set; }

        public string Version { get; set; }

        public string GreedVersion { get; set; }

        public string SinsVersion { get; set; }

        public OnlineListItem(Metadata m)
        {
            Name = m.Name;
            Version = m.Version.ToString();
            GreedVersion = m.GreedVersion.ToString();
            SinsVersion = m.SinsVersion.ToString();
        }
    }
}
