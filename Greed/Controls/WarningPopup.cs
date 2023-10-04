using Greed.Interfaces;
using Greed.Models;
using Greed.Models.Online;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Greed.Controls
{
    /// <summary>
    /// This is separated out for the sake of unit testing ModManager proper.
    /// </summary>
    public class WarningPopup : IWarningPopup
    {
        public MessageBoxResult Conflicts(Mod m, List<Mod> conflicts)
        {
            var rows = string.Join('\n', conflicts.Select(c => "- " + c.Meta.Name));
            var plural = conflicts.Count > 1 ? "s" : "";
            return MessageBox.Show($"A known conflict was detected while enabling {m.Meta.Name}:\n{rows}\n\nTo continue activating {m.Meta.Name}, would you like to disable the conflicting mod{plural}?", "Conflict Detected", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
        }

        public MessageBoxResult Dependents(Mod m, List<Mod> dependents)
        {
            var rows = string.Join('\n', dependents.Select(c => "- " + c.Meta.Name));
            var plural = dependents.Count > 1 ? "s" : "";
            return MessageBox.Show($"{m.Meta.Name} has dependents that require it:\n{rows}\n\nTo continue deactivating {m.Meta.Name}, would you like to disable the dependent mod{plural}?", "Dependent Detected", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
        }

        public MessageBoxResult Dependencies(Mod m, List<string> violations, List<Mod> dependencies)
        {
            var plural = dependencies.Count > 1 ? "ies" : "y";
            var rows = string.Join('\n', violations);
            return MessageBox.Show($"{m.Meta.Name} has missing depencies that it requires:\n{rows}\n\nTo continue activating {m.Meta.Name}, would you like to enable its installed dependenc{plural}?", "Dependency Detected", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
        }

        public MessageBoxResult DependencyOrder(Mod m, List<string> violations)
        {
            var plural = violations.Count > 1 ? "ies" : "y";
            var rows = string.Join('\n', violations);
            return MessageBox.Show($"Moving {m.Meta.Name} would cause a load order violation:\n{rows}\n\nTo continue moving {m.Meta.Name}, would you like to hoist its dependenc{plural} as well?", "Dependency Load Order Violation", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
        }

        public MessageBoxResult DependentOrder(Mod m, List<string> violations)
        {
            var plural = violations.Count > 1 ? "s" : "";
            var rows = string.Join('\n', violations);
            return MessageBox.Show($"Moving {m.Meta.Name} would cause a load order violation:\n{rows}\n\nTo continue moving {m.Meta.Name}, would you like to lower its dependent{plural} as well?", "Dependent Load Order Violation", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
        }

        public MessageBoxResult PredecessorOrder(Mod m, List<string> violations)
        {
            var plural = violations.Count > 1 ? "s" : "";
            var rows = string.Join('\n', violations);
            return MessageBox.Show($"Moving {m.Meta.Name} would cause a load order violation:\n{rows}\n\nTo continue moving {m.Meta.Name}, would you like to hoist its predecessor{plural} as well?", "Predecessor Load Order Violation", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
        }

        public MessageBoxResult SuccessorOrder(Mod m, List<string> violations)
        {
            var plural = violations.Count > 1 ? "s" : "";
            var rows = string.Join('\n', violations);
            return MessageBox.Show($"Moving {m.Meta.Name} would cause a load order violation:\n{rows}\n\nTo continue moving {m.Meta.Name}, would you like to lower its successor{plural} as well?", "Successor Load Order Violation", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
        }

        public void FailedToResolveDependencies(List<string> violations)
        {
            MessageBox.Show("Unable to resolve all dependencies:\n" + string.Join("\n", violations));
        }
        public MessageBoxResult ChainedInstall(OnlineMod mod, Dependency dep)
        {
            return MessageBox.Show($"{mod.Name} has {dep} listed as a dependency.\n\nWould you like to install it now?", "Chained Installation Detected", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
        }
        public MessageBoxResult ChainedUninstall(Mod mod, Mod dependent)
        {
            return MessageBox.Show($"You wish to uninstall {mod.Meta.Name}, but it is a dependency for {dependent.Meta.Name}.\n\nWould you like to uninstall {dependent.Meta.Name} as well?", "Chained Uninstallation Detected", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
        }
    }
}
