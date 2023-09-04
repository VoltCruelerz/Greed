using Greed.Models;
using System.Collections.Generic;
using System.Windows;

namespace Greed.Interfaces
{
    public interface IWarningPopup
    {
        // Message box warnings
        public MessageBoxResult WarnOfConflicts(Mod m, List<Mod> conflicts);
        public MessageBoxResult WarnOfDependents(Mod m, List<Mod> dependents);
        public MessageBoxResult WarnOfDependencies(Mod m, List<string> violations, List<Mod> dependents);
        public MessageBoxResult WarnOfDependencyOrder(Mod m, List<string> violations);
        public void WarnFailedToResolveDependencies(List<string> violations);
    }
}
