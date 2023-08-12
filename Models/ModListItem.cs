namespace Greed.Models
{
    public class ModListItem
    {
        public string Id { get; set; }

        public string Active { get; set; }

        public string Name { get; set; }

        public string Version { get; set; }

        public string SinsVersion { get; set; }

        public ModListItem(Mod m)
        {
            Id = m.Id;
            Active = m.IsActive ? "✓" : " ";
            Name = m.Meta.Name;
            Version = m.Meta.Version;
            SinsVersion = m.Meta.SinsVersion;
        }
    }
}
