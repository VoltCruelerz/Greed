using System.Linq;

namespace Greed.Models.ListItem
{
    public class ModListItem
    {
        public string Id { get; set; }

        public string Active { get; set; }

        public string Name { get; set; }

        public string Version { get; set; }

        public string GreedVersion { get; set; }

        public string SinsVersion { get; set; }

        public bool IsLegal { get; set; }

        public ModListItem(Mod m, MainWindow window)
        {
            Id = m.Id;
            Active = m.IsActive ? "✓" : " ";
            Name = m.Meta.Name;
            Version = m.Meta.Version.ToString();
            GreedVersion = m.Meta.GreedVersion.ToString();
            SinsVersion = m.Meta.SinsVersion.ToString();

            var versionViolation = m.Meta.IsLegalVersion();
            if (versionViolation.Contains("Mod"))
            {
                Version = "⚠ " + Version;
            }
            if (versionViolation.Contains("Greed"))
            {
                GreedVersion = "⚠ " + GreedVersion;
            }
            if (versionViolation.Contains("Sins"))
            {
                SinsVersion = "⚠ " + SinsVersion;
            }

            if (versionViolation.Any())
            {
                window.PrintAsync("WARNING - Incompatible Version: " + m.Meta.Name + "\r\n- " + string.Join("\r\n- ", versionViolation));
            }
        }
    }
}
