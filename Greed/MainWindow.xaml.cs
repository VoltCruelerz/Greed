using Greed.Controls.Diff;
using Greed.Controls.Online;
using Greed.Models;
using Greed.Models.Json;
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
using System.Windows.Media;

namespace Greed
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly string LogPath = Directory.GetCurrentDirectory() + "\\log.txt";
        private static readonly string LogPrevPath = Directory.GetCurrentDirectory() + "\\log_prev.txt";
        private const int ModPageIndex = 0;
        private const int SettingsPageIndex = 1;
        private int SelectedTabIndex = 0;

        private readonly ModManager Manager = new();
        private List<Mod> Mods = new();
        private Mod? SelectedMod;
        private readonly List<JsonSource> AllSources = new();
        private JsonSource? SelectedSource;

        private readonly SolidColorBrush Valid = new(Colors.White);
        private readonly SolidColorBrush Invalid = new(Colors.Pink);

        private Mod? DragMod;
        private Mod? DestMod;
        private string SearchQuery = string.Empty;
        private bool SearchActive = false;

        public MainWindow()
        {
            InitializeComponent();
            PrintSync("Components Loaded");

            // Shift the log history.
            if (File.Exists(LogPath))
            {
                if (File.Exists(LogPrevPath))
                {
                    File.Delete(LogPrevPath);
                }
                File.Move(LogPath, LogPrevPath);
            }

            TxtLocalModInfo.Document.Blocks.Clear();
            TxtLocalModInfo.AppendText("Select a mod to view details about it.");

            string? modDir = ConfigurationManager.AppSettings["modDir"];
            txtModsDir.Text = modDir ?? "";
            if (modDir == null || !Directory.Exists(modDir))
            {
                txtModsDir.Background = Invalid;
                Tabs.SelectedIndex = SettingsPageIndex;
            }
            string? sinsDir = ConfigurationManager.AppSettings["sinsDir"];
            txtSinsDir.Text = sinsDir ?? "";
            if (sinsDir == null || !Directory.Exists(sinsDir))
            {
                txtSinsDir.Background = Invalid;
                Tabs.SelectedIndex = SettingsPageIndex;
            }
            PrintSync("Directories Explored");

            // Only load mods if there's something to load.
            if (Tabs.SelectedIndex != SettingsPageIndex)
            {
                ReloadModListFromDisk();
            }

            pgbProgress.Value = 0;// Ranges [0, 100].
        }

        private void ReloadModListFromDisk()
        {
            PrintSync("Loading Settings...");
            string modDir = ConfigurationManager.AppSettings["modDir"]!;
            string sinsDir = ConfigurationManager.AppSettings["sinsDir"]!;
            PrintSync($"Mod Dir: {modDir}");
            PrintSync($"Sins II Dir: {sinsDir}");
            try
            {
                var greedVersion = Assembly.GetExecutingAssembly().GetName().Version!;
                this.Title = $"Greed Mod Loader v{greedVersion} (Detected Sins II v{FileVersionInfo.GetVersionInfo(sinsDir + "\\sins2.exe").FileVersion})";
            }
            catch (Exception)
            {
                CriticalAlertPopup("No SinsII", "sins2.exe could not be found at the specified location. Please double check that it is within the place indicated by the App.config.");
                viewModList.Items.Clear();
                Tabs.SelectedIndex = SettingsPageIndex;
                return;
            }

            PrintSync("Loading mods...");
            try
            {
                Mods = Manager.LoadGreedyMods();
            }
            catch (Exception e)
            {
                CriticalAlertPopup("Mod Load Error", "Unable to locate all files.\n" + e.Message + "\n" + e.StackTrace);
                viewModList.Items.Clear();
                Tabs.SelectedIndex = SettingsPageIndex;
                return;
            }
            PrintSync("Load succeeded.");

            RefreshModListUI();
            ReloadSourceFileList();
            PrintSync("Refresh Complete");
        }

        private void RefreshModListUI()
        {
            PrintSync($"RefreshModListUI for {Mods.Count} mods.");
            viewModList.Items.Clear();
            Mods
                .Where(m => string.IsNullOrWhiteSpace(SearchQuery)
                    || m.Id.Contains(SearchQuery)
                    || m.Readme.Contains(SearchQuery)
                    || m.Meta.Name.Contains(SearchQuery)
                    || m.Meta.Author.Contains(SearchQuery)
                    || m.Meta.Description.Contains(SearchQuery))
                .Where(m => !SearchActive || m.IsActive)
                .ToList()
                .ForEach(m => viewModList.Items.Add(new ModListItem(m, this)));
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
            var newSelection = Mods.First(p => p.Id == item.Id);

            // Ignore the user re-clicking.
            if (newSelection == SelectedMod)
            {
                // Cleanup the button text and return.
                cmdToggle.IsEnabled = true;
                cmdToggle.Content = SelectedMod.IsActive ? "Deactivate" : "Activate";
                return;
            }
            SelectedMod = newSelection;

            // If there is no mod currently selected, start the drag process.
            if (DragMod == null)
            {
                DragMod = SelectedMod;
            }
            else
            {
                DestMod = SelectedMod;
            }

            TxtLocalModInfo.SetContent(SelectedMod.Meta, SelectedMod);

            // It starts disabled since nothing is selected.
            cmdToggle.IsEnabled = true;
            cmdToggle.Content = SelectedMod.IsActive ? "Deactivate" : "Activate";
            cmdDiff.IsEnabled = false;

            ReloadSourceFileList();
        }

        private void ModList_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DestMod == null || DragMod == DestMod)
            {
                // We aren't or aborted drag-and-drop.
                DragMod = null;
                DestMod = null;
                return;
            }

            if (DragMod == null)
            {
                throw new InvalidOperationException("DragMod was somehow null.");
            }

            // We *are* actually reordering now.
            Manager.MoveMod(Mods, DragMod, Mods.IndexOf(DestMod));
            DragMod = null;
            DestMod = null;
            Manager.SetGreedyMods(Mods.Where(m => m.IsActive).ToList());
            RefreshModListUI();
        }

        private void ModList_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            DragMod = null;
            DestMod = null;
        }

        private void Toggle_Click(object sender, RoutedEventArgs e)
        {
            PrintSync("Toggle_Click()");
            SelectedMod!.SetModActivity(Mods, !SelectedMod.IsActive);

            string modDir = ConfigurationManager.AppSettings["modDir"]!;
            try
            {
                Manager.SetGreedyMods(Mods.Where(m => m.IsActive).ToList());
            }
            catch (Exception ex)
            {
                CriticalAlertPopup("Mod Set Error", "Unable to locate all files.\n" + ex.Message + "\n" + ex.StackTrace);
                return;
            }
            RefreshModListUI();
            ReselectSelection();
        }

        private void ToggleAll_Click(object sender, RoutedEventArgs e)
        {
            PrintSync("ToggleAll_Click()");
            var areAllActive = Mods.All(m => m.IsActive);
            Mods.ForEach(m => m.SetModActivity(Mods, !areAllActive));

            string modDir = ConfigurationManager.AppSettings["modDir"]!;
            try
            {
                Manager.SetGreedyMods(Mods.Where(m => m.IsActive).ToList());
            }
            catch (Exception ex)
            {
                CriticalAlertPopup("Mod Set Error", "Unable to locate all files.\n" + ex.Message + "\n" + ex.StackTrace);
                return;
            }
            RefreshModListUI();
            ReselectSelection();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            PrintSync("Refresh_Click()");
            ReloadModListFromDisk();
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            cmdExport.IsEnabled = false;
            ReselectSelection();
            ReloadModListFromDisk();

            PrintSync("Exporting...");
            try
            {
                var active = Mods.Where(m => m.IsGreedy && m.IsActive).ToList();
                Manager.ExportGreedyMods(active, pgbProgress, this, (exportSucceeded) =>
                {
                    if (exportSucceeded)
                    {
                        PrintAsync("Export Complete");
                        var response = MessageBox.Show("Greedy mods are now active. Would you like to start sinning?", "Export Success", MessageBoxButton.YesNo, MessageBoxImage.Information);

                        if (response == MessageBoxResult.Yes)
                        {
                            Play();
                        }
                    }
                    else
                    {
                        CriticalAlertPopup("Mod Export Error", "Unable to locate all files.\nSee log for details.");
                    }
                });
            }
            catch (Exception ex)
            {
                CriticalAlertPopup("Failed to Export", ex);
            }
            finally
            {
                cmdExport.Dispatcher.Invoke(() =>
                {
                    cmdExport.IsEnabled = true;
                });
            }
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

        private void CriticalAlertPopup(string title, Exception ex)
        {
            CriticalAlertPopup(title, ex.Message + "\n" + ex.StackTrace);
        }

        private void CriticalAlertPopup(string title, string message)
        {
            PrintAsync(message);
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ReloadSourceFileList()
        {
            SelectedSource = null;
            viewFileList.Items.Clear();
            if (SelectedMod != null)
            {
                AllSources.Clear();

                // Json
                AllSources.AddRange(SelectedMod.Brushes);
                AllSources.AddRange(SelectedMod.Colors);
                AllSources.AddRange(SelectedMod.Cursors);
                AllSources.AddRange(SelectedMod.DeathSequences);
                AllSources.AddRange(SelectedMod.Effects);
                AllSources.AddRange(SelectedMod.Fonts);
                AllSources.AddRange(SelectedMod.GravityWellProps);
                AllSources.AddRange(SelectedMod.Gui);
                AllSources.AddRange(SelectedMod.MeshMaterials);
                AllSources.AddRange(SelectedMod.PlayerColors);
                AllSources.AddRange(SelectedMod.PlayerIcons);
                AllSources.AddRange(SelectedMod.PlayerPortraits);
                AllSources.AddRange(SelectedMod.Skyboxes);
                AllSources.AddRange(SelectedMod.TextureAnimations);
                AllSources.AddRange(SelectedMod.Uniforms);
                AllSources.AddRange(SelectedMod.Entities);
                AllSources.AddRange(SelectedMod.LocalizedTexts);

                // Non-Json
                //AllSources.AddRange(SelectedMod.Meshes);
                //AllSources.AddRange(SelectedMod.Scenarios);
                //AllSources.AddRange(SelectedMod.Shaders);
                //AllSources.AddRange(SelectedMod.Sounds);
                //AllSources.AddRange(SelectedMod.Textures);

                try
                {
                    AllSources.ForEach(p => viewFileList.Items.Add(new SourceListItem(p)));
                }
                catch (Exception ex)
                {
                    CriticalAlertPopup("Failed to Load Mod", ex);
                }
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
            PrintSync("Selected " + SelectedSource?.Mergename);
            cmdDiff.IsEnabled = true;
        }

        private void Diff_Click(object sender, RoutedEventArgs e)
        {
            PrintSync("Diff_Click()");
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

        private void TxtSinsDir_TextChanged(object sender, TextChangedEventArgs e)
        {
            var txt = (TextBox)sender;
            string newVal = txt.Text.Replace("/", "\\");
            var exists = Directory.Exists(newVal);
            txt.Background = exists ? Valid : Invalid;
            if (exists && newVal != ConfigurationManager.AppSettings["sinsDir"])
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings["sinsDir"].Value = newVal;
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
        }

        private void TxtModDir_TextChanged(object sender, TextChangedEventArgs e)
        {
            var txt = (TextBox)sender;
            string newVal = txt.Text.Replace("/", "\\");
            var exists = Directory.Exists(newVal);
            txt.Background = exists ? Valid : Invalid;
            if (exists && newVal != ConfigurationManager.AppSettings["modDir"])
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings["modDir"].Value = newVal;
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
        }

        private void Tabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var control = ((TabControl)sender);
            var i = control.SelectedIndex;
            if (i != SelectedTabIndex)
            {
                PrintSync($"Tabs_SelectionChanged(): {i}");
                if (i == ModPageIndex && SelectedTabIndex != ModPageIndex)
                {
                    // We have to set this BEFORE the file I/O so we don't start trying to load a second time while still in the first one.
                    // (This can happen if we focus an element and then switch tabs twice.)
                    SelectedTabIndex = i;
                    ReloadModListFromDisk();
                }
                else
                {
                    SelectedTabIndex = i;
                }
            }
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            Play();
        }

        /// <summary>
        /// Start the game.
        /// </summary>
        private void Play()
        {
            var execPath = ConfigurationManager.AppSettings["sinsDir"]! + "\\sins2.exe";
            PrintAsync("Executing: " + execPath);
            Process.Start(new ProcessStartInfo(execPath)
            {
                WorkingDirectory = ConfigurationManager.AppSettings["sinsDir"]!
            });
        }

        /// <summary>
        /// Print synchronously.
        /// </summary>
        /// <param name="str"></param>
        public void PrintSync(string str)
        {
            txtLog.Text = txtLog.Text.Any()
                ? txtLog.Text + '\n' + str
                : str;
            txtLog.ScrollToEnd();
            File.AppendAllText(LogPath, str + "\r\n");
        }

        /// <summary>
        /// Invoke the print function when possible.
        /// </summary>
        /// <param name="str"></param>
        public void PrintAsync(string str)
        {
            Dispatcher.Invoke(() => PrintSync(str));
        }

        private void TxtSearchMods_TextChanged(object sender, TextChangedEventArgs e)
        {
            var txt = (TextBox)sender;
            SearchQuery = txt.Text;
            RefreshModListUI();
        }

        private void CheckActive_Toggle(object sender, RoutedEventArgs e)
        {
            var cbx = (CheckBox)sender;
            SearchActive = cbx.IsChecked ?? false;
            RefreshModListUI();
        }

        private async void CmdOnline_Click(object sender, RoutedEventArgs e)
        {
            var onlinePopup = new OnlineWindow(await OnlineListing.GetOnlineListing(this), this);
            onlinePopup.ShowDialog();
        }

        private void MenuUninstall_Click(object sender, RoutedEventArgs e)
        {
            var response = MessageBox.Show($"Are you sure you want to uninstall {SelectedMod!.Meta.Name}?", $"Uninstalling {SelectedMod!.Meta.Name}", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (response == MessageBoxResult.Yes)
            {
                PrintSync($"Uninstalling {SelectedMod!.Meta.Name}...");
                string modDir = ConfigurationManager.AppSettings["modDir"]!;
                var path = modDir + "\\" + SelectedMod.Id;
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
                ReloadModListFromDisk();
            }
        }
    }
}
