using Greed.Models;
using Greed.Models.Online;
using System.Collections.Generic;
using System.Windows;

namespace Greed.Interfaces
{
    public interface IWarningPopup
    {
        // Message box warnings
        public MessageBoxResult Conflicts(Mod m, List<Mod> conflicts);
        public MessageBoxResult Dependents(Mod m, List<Mod> dependents);
        public MessageBoxResult Dependencies(Mod m, List<string> violations, List<Mod> dependents);
        public MessageBoxResult DependencyOrder(Mod m, List<string> violations);
        public MessageBoxResult DependentOrder(Mod m, List<string> violations);
        public MessageBoxResult PredecessorOrder(Mod m, List<string> violations);
        public MessageBoxResult SuccessorOrder(Mod m, List<string> violations);
        public void FailedToResolveDependencies(List<string> violations);
        public MessageBoxResult ChainedInstall(OnlineMod mod, Dependency dep);
    }
}
