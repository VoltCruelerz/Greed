using Greed.Models;
using Greed.Models.Vault;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greed.Interfaces
{
    public interface IVault
    {
        public void ArchiveActiveOnly(List<Mod> allMods);
        public string ExportPortable(string name, List<Mod> allMods);
    }
}
