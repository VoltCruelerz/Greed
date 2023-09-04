using Greed.Models;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Greed.Interfaces
{
    public interface IModManager
    {
        public List<Mod> LoadGreedyMods();
        public void SetGreedyMods(List<Mod> active);
        public void ExportGreedyMods(List<Mod> active, ProgressBar pgbProgress, MainWindow window, Action<bool> callback);
        public void MoveMod(List<Mod> allMods, Mod mover, int destination);
    }
}
