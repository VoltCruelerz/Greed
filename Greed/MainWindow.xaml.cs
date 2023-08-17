using Greed.Models;
using Greed.Models.JsonSource;
using Greed.Models.ListItem;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Greed
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Mod> Mods = new();
        private Mod? SelectedMod;
        private List<Source> AllSources = new();
        private Source? SelectedSource;

        public MainWindow()
        {
            Debug.WriteLine("Main Window");
            InitializeComponent();

            txtInfo.Document.Blocks.Clear();
            txtInfo.AppendText("Select a mod to view details about it.");
            Debug.WriteLine("Load Done");

            string? modDir = ConfigurationManager.AppSettings["modDir"];
            if (modDir == null || !Directory.Exists(modDir))
            {
                CriticalAlertPopup("Configuration Error", "You need to set the modDir element in Greed.dll.config file to your mod directory.");
                return;
            }
            string? sinsDir = ConfigurationManager.AppSettings["sinsDir"];
            if (sinsDir == null || !Directory.Exists(sinsDir))
            {
                CriticalAlertPopup("Configuration Error", "You need to set the sinsDir element in Greed.dll.config file to where sins2.exe lives.");
                return;
            }

            RefreshModList();
        }

        private void RefreshModList()
        {
            Debug.WriteLine("Loading Settings...");
            string modDir = ConfigurationManager.AppSettings["modDir"]!;
            string sinsDir = ConfigurationManager.AppSettings["sinsDir"]!;
            Debug.WriteLine($"Mod Dir: {modDir}");
            Debug.WriteLine($"Sins II Dir: {sinsDir}");
            try
            {
                var greedVersion = Assembly.GetExecutingAssembly().GetName().Version!;
                this.Title = $"Greed Mod Loader v{greedVersion} (Detected Sins II v{FileVersionInfo.GetVersionInfo(sinsDir + "\\sins2.exe").FileVersion})";
            }
            catch (Exception)
            {
                CriticalAlertPopup("No SinsII", "sins2.exe could not be found at the specified location. Please double check that it is within the place indicated by the App.config.");
                return;
            }

            Debug.WriteLine("Loading mods...");
            try
            {
                Mods = ModManager.LoadGreedyMods();
            }
            catch (Exception e)
            {
                CriticalAlertPopup("Mod Load Error", "Unable to locate all files.\n" + e.Message + "\n" + e.StackTrace);
                return;
            }

            viewModList.Items.Clear();
            Mods.ForEach(m => viewModList.Items.Add(new ModListItem(m)));
            RefreshSourceList();
        }

        private void ModList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // This gets hit when refreshing the display.
            if (e.AddedItems.Count == 0)
            {
                cmdToggle.IsEnabled = false;
                cmdToggle.Content = "Toggle";
                return;
            }
            var item = (ModListItem)e.AddedItems[0]!;
            SelectedMod = Mods.First(p => p.Id == item.Id);

            var doc = new FlowDocument(new Paragraph(new Run($"{SelectedMod.Meta.Name} v{SelectedMod.Meta.Version} (Sins {SelectedMod.Meta.SinsVersion})")
            {
                FontWeight = FontWeights.Bold
            }));
            doc.Blocks.Add(new Paragraph(new Run($"by {SelectedMod.Meta.Author}")
            {
                FontStyle = FontStyles.Italic
            }));
            doc.Blocks.Add(new Paragraph(new Run(SelectedMod.Meta.Url)
            {
                TextDecorations = TextDecorations.Underline
            }));
            doc.Blocks.Add(new Paragraph(new Run(SelectedMod.Meta.Description)));
            doc.Blocks.Add(new Paragraph(new Run(SelectedMod.Readme)));
            txtInfo.Document = doc;

            // It starts disabled since nothing is selected.
            cmdToggle.IsEnabled = true;
            cmdToggle.Content = SelectedMod.IsActive ? "Deactivate" : "Activate";
            cmdDiff.IsEnabled = false;

            RefreshSourceList();
        }

        private void Toggle_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Toggle_Click()");
            SelectedMod!.IsActive = !SelectedMod.IsActive;

            string modDir = ConfigurationManager.AppSettings["modDir"]!;
            try
            {
                ModManager.SetGreedyMods(Mods.Where(m => m.IsActive).ToList());
            }
            catch (Exception ex)
            {
                CriticalAlertPopup("Mod Set Error", "Unable to locate all files.\n" + ex.Message + "\n" + ex.StackTrace);
                return;
            }
            RefreshModList();
            ReselectSelection();
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            cmdExport.IsEnabled = false;
            RefreshModList();
            ReselectSelection();

            try
            {
                ModManager.ExportGreedyMods(Mods.Where(m => m.IsGreedy && m.IsActive).ToList());
                MessageBox.Show("Greedy mods are now active. Have fun Sinning!", "Export Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                CriticalAlertPopup("Mod Export Error", "Unable to locate all files.\n" + ex.Message + "\n" + ex.StackTrace);
                return;
            }

            cmdExport.IsEnabled = true;
        }

        private void ReselectSelection()
        {
            SelectedMod = Mods.Find(m => m.Id == SelectedMod?.Id);
            if (SelectedMod != null)
            {
                var index = Mods.IndexOf(SelectedMod!);
                viewModList.SelectedItem = SelectedMod;
                viewModList.SelectedIndex = index;
            }
        }

        private void CriticalAlertPopup(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            this.Close();
        }

        private void RefreshSourceList()
        {
            SelectedSource = null;
            viewFileList.Items.Clear();
            if (SelectedMod != null)
            {
                AllSources.Clear();
                AllSources.AddRange(SelectedMod.Brushes);
                AllSources.AddRange(SelectedMod.Colors);
                AllSources.AddRange(SelectedMod.Cursors);
                AllSources.AddRange(SelectedMod.DeathSequences);
                AllSources.AddRange(SelectedMod.Effects);
                AllSources.AddRange(SelectedMod.Fonts);
                AllSources.AddRange(SelectedMod.GravityWellProps);
                AllSources.AddRange(SelectedMod.Gui);
                AllSources.AddRange(SelectedMod.MeshMaterials);
                AllSources.AddRange(SelectedMod.Meshes);
                AllSources.AddRange(SelectedMod.PlayerColors);
                AllSources.AddRange(SelectedMod.PlayerIcons);
                AllSources.AddRange(SelectedMod.PlayerPortraits);
                AllSources.AddRange(SelectedMod.Scenarios);
                AllSources.AddRange(SelectedMod.Shaders);
                AllSources.AddRange(SelectedMod.Skyboxes);
                AllSources.AddRange(SelectedMod.Sounds);
                AllSources.AddRange(SelectedMod.TextureAnimations);
                AllSources.AddRange(SelectedMod.Textures);
                AllSources.AddRange(SelectedMod.Uniforms);
                AllSources.AddRange(SelectedMod.Entities);
                AllSources.AddRange(SelectedMod.LocalizedTexts);
                AllSources.ForEach(p => viewFileList.Items.Add(new SourceListItem(p)));
            }
        }

        private void FileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
            {
                return;
            }
            var item = (SourceListItem)e.AddedItems[0]!;
            SelectedSource = AllSources.Find(p => p.Mergename == item.Name && p.Folder == item.Folder);
            Debug.WriteLine("Selected " + SelectedSource?.Mergename);
            cmdDiff.IsEnabled = true;
        }

        private void Diff_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Diff_Click()");
            try
            {
                var diffPopup = new DiffWindow(SelectedSource!);
                diffPopup.ShowDialog();
            }
            catch (Exception ex)
            {
                CriticalAlertPopup("Failed to Load Diff", ex.Message + "\n" + ex.StackTrace);
            }
        }
    }
}
