using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Greed.Models;
using Greed.Models.EnabledMods;
using Newtonsoft.Json;

namespace Greed
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Mod> Mods = new();
        private Mod? Selected;

        public MainWindow()
        {
            Debug.WriteLine("Main Window");
            InitializeComponent();
            Debug.WriteLine("Load Done");

            RefreshModList();
        }

        private void RefreshModList()
        {
            Debug.WriteLine("Loading Settings...");
            string modDir = ConfigurationManager.AppSettings["modDir"]!;
            string sinsDir = ConfigurationManager.AppSettings["sinsDir"]!;
            Debug.WriteLine($"Mod Dir: {modDir}");
            Debug.WriteLine($"Sins II Dir: {sinsDir}");

            Debug.WriteLine("Loading mods...");
            Mods = ModManager.LoadGreedyMods();

            viewModList.Items.Clear();
            Mods.ForEach(m => viewModList.Items.Add(new ModListItem(m)));
        }

        private void viewModList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // This gets hit when refreshing the display.
            if (e.AddedItems.Count == 0)
            {
                cmdToggle.IsEnabled = false;
                cmdToggle.Content = "Toggle";
                return;
            }
            var item = (ModListItem)e.AddedItems[0]!;
            Selected = Mods.First(p => p.Id == item.Id);

            var doc = new FlowDocument(new Paragraph(new Run($"{Selected.Meta.Name} v{Selected.Meta.Version} (Sins {Selected.Meta.SinsVersion})")
            {
                FontWeight = FontWeights.Bold
            }));
            doc.Blocks.Add(new Paragraph(new Run($"by {Selected.Meta.Author}")
            {
                FontStyle = FontStyles.Italic
            }));
            doc.Blocks.Add(new Paragraph(new Run(Selected.Meta.Url)
            {
                TextDecorations = TextDecorations.Underline
            }));
            doc.Blocks.Add(new Paragraph(new Run(Selected.Meta.Description)));
            txtInfo.Document = doc;

            // It starts disabled since nothing is selected.
            cmdToggle.IsEnabled = true;
            cmdToggle.Content = Selected.IsActive ? "Deactivate" : "Activate";
        }

        private void cmdToggle_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Activate_Click()");
            Selected!.IsActive = !Selected.IsActive;
            string modDir = ConfigurationManager.AppSettings["modDir"]!;
            ModManager.SetGreedyMods(Mods.Where(m => m.IsActive).ToList());
            RefreshModList();

            // Reselect the selection for the user.
            Selected = Mods.Find(m => m.Id == Selected.Id);
            var index = Mods.IndexOf(Selected!);
            viewModList.SelectedItem = Selected;
            viewModList.SelectedIndex = index;
        }

        //private void ExportModList()
        //{
        //    // If enabled path doesn't exist yet, make it.
        //    var enabledPath = ConfigurationManager.AppSettings["modDir"]! + "\\enabled_mods.json";
        //    List<string> enabledModFolders;
        //    if (File.Exists(enabledPath))
        //    {
        //        var enabled = JsonConvert.DeserializeObject<EnabledMods>(File.ReadAllText(enabledPath))!;
        //        enabledModFolders = enabled.ModKeys.Select(p => p.Name).ToList();
        //    }
        //    else
        //    {
        //        enabledModFolders = new List<string>();
        //        File.WriteAllText(enabledPath, "{ \"mod_keys\": [] }");
        //    }
        //}
    }
}
