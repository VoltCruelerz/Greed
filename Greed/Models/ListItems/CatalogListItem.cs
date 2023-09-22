using Greed.Extensions;
using Greed.Models.Online;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;

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

        public CatalogListItem(OnlineMod m, Dictionary<string, Version> installedModVersions)
        {
            Name = m.Name;
            Id = m.Id ?? m.Name;
            Author = m.Author;

            Version = m.Latest.ToString();
            if (installedModVersions.ContainsKey(Id) && installedModVersions[Id].IsOlderThan(m.Latest))
            {
                Version = Constants.UNI_READY_FOR_UPDATE + " " + Version;
            }

            GreedVersion = m.Live.GreedVersion.ToString();
            var installedGreedVersion = Assembly.GetExecutingAssembly().GetName().Version!;
            if (installedGreedVersion.IsOlderThan(m.Live.GreedVersion))
            {
                GreedVersion = Constants.UNI_WARN + " " + GreedVersion;
            }

            SinsVersion = m.Live.SinsVersion.ToString();
            var sinsDir = ConfigurationManager.AppSettings["sinsDir"]!;
            var installedSinsVersion = new Version(FileVersionInfo.GetVersionInfo(sinsDir + "\\sins2.exe").FileVersion!);
            if (installedSinsVersion.IsOlderThan(m.Live.SinsVersion))
            {
                SinsVersion = Constants.UNI_WARN + " " + SinsVersion;
            }

            LastUpdated = !string.IsNullOrEmpty(m.Live.DateAdded)
                ? m.Live.DateAdded
                : DateTime.Today.ToString();
            IsInstalled = ModManager.IsModInstalled(Id)
                ? Constants.UNI_INSTALLED
                : string.IsNullOrEmpty(m.Live.Download)
                    ? Constants.UNI_NO_INSTALL
                    : Constants.UNI_READY_FOR_INSTALL;
        }
    }
}
