using Greed.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greed
{
    static class ModLoader
    {
        public static List<Mod> LoadGreedyMods(string modDir)
        {
            var modDirs = Directory.GetDirectories(modDir);
            var 

            return modDirs.Select(d => new Mod(d)).Where(m => m.IsGreedy).ToList();
        }
    }
}
