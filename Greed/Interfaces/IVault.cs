using Greed.Models;
using System.Collections.Generic;

namespace Greed.Interfaces
{
    public interface IVault
    {
        public void ArchiveActiveOnly(List<Mod> allMods);
        public string ExportPortable(string name, List<Mod> allMods);
    }
}
