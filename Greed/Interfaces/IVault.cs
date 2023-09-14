using Greed.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greed.Interfaces
{
    public interface IVault
    {
        public void ExportActiveOnly(List<Mod> allMods);
    }
}
