﻿using Greed.Controls.Diff;
using Greed.Controls.Online;
using Greed.Models;
using Greed.Models.Json;
using Greed.Models.ListItem;
using Greed.Models.Online;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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
        private OnlineCatalog Catalog = new();
        private TabItem? SelectedTab = null;
        private readonly HashSet<string> InvalidSettings = new();

        public MainWindow()
        {
            InitializeComponent();
            SetTitle();
            ReloadCatalog();
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

            // Populate the config fields.
            PopulateConfigField(txtModsDir, "modDir", Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "Local", "sins2", "mods"));
            PopulateConfigField(txtSinsDir, "sinsDir", "C:\\Program Files\\Epic Games\\SinsII");
            PopulateConfigField(txtDownloadDir, "downDir", Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),  "Downloads"));
            PopulateCbx(CbxChannel, "channel");
            PrintSync("Directories Explored");

            // Only load mods if there's something to load.
            if (SelectedTab != TabSettings)
            {
                ReloadModListFromDisk();
            }

            pgbProgress.Value = 0;// Ranges [0, 100].
        }

        private void SetTitle()
        {
            string modDir = ConfigurationManager.AppSettings["modDir"]!;
            string sinsDir = ConfigurationManager.AppSettings["sinsDir"]!;
            PrintSync($"Mod Dir: {modDir}");
            PrintSync($"Sins II Dir: {sinsDir}");
            try
            {
                var greedVersion = Assembly.GetExecutingAssembly().GetName().Version!;
                var updateStr = greedVersion.CompareTo(Catalog.LatestGreed) < 0
                    ? $" - ⚠ Greed v{Catalog.LatestGreed} is now available! ⚠"
                    : "";
                Title = $"Greed Mod Loader v{greedVersion} (Detected Sins II v{FileVersionInfo.GetVersionInfo(sinsDir + "\\sins2.exe").FileVersion}){updateStr}";
            }
            catch (Exception)
            {
                viewModList.Items.Clear();
                Tabs.SelectedItem = TabSettings;
                return;
            }
        }

        private void SetTitleAsync()
        {
            Dispatcher.Invoke(() => SetTitle());
        }

        #region Reload Mods
        /// <summary>
        /// Reloads the list of installed mods
        /// </summary>
        private void ReloadModListFromDisk()
        {
            PrintSync("Loading Settings...");
            PrintSync("Loading mods...");
            try
            {
                Mods = Manager.LoadGreedyMods();
            }
            catch (Exception e)
            {
                CriticalAlertPopup("Mod Load Error", "Unable to locate all files.\n" + e.Message + "\n" + e.StackTrace);
                viewModList.Items.Clear();
                Tabs.SelectedItem = TabSettings;
                return;
            }
            PrintSync("Load succeeded.");

            RefreshModListUI();
            ReloadSourceFileList();
            PrintSync("Refresh Complete");
        }

        public void ReloadModListFromDiskAsync()
        {
            Dispatcher.Invoke(() => ReloadModListFromDisk());
        }

        /// <summary>
        /// Reloads the mod list UI
        /// </summary>
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
                .ForEach(m => viewModList.Items.Add(new ModListItem(m, this, Catalog)));
        }

        /// <summary>
        /// Reloads the mod list UI from non-main threads
        /// </summary>
        public void RefreshModListUIAsync()
        {
            Dispatcher.Invoke(() => RefreshModListUI());
        }

        /// <summary>
        /// Reloads the update catalog in the background.
        /// </summary>
        public void ReloadCatalog()
        {
            Task.Run(async () =>
            {
                Catalog = await OnlineCatalog.GetOnlineListing(this);
                RefreshModListUIAsync();
                SetTitleAsync();
            }).ConfigureAwait(false);
        }
        #endregion

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

            // Load the text field.
            try
            {
                TxtLocalModInfo.SetContent(SelectedMod.Meta, SelectedMod);
            }
            catch (Exception ex)
            {
                CriticalAlertPopup("Metadata Error", ex);
            }

            UpdateRightClickMenuOptions();

            // It starts disabled since nothing is selected.
            cmdToggle.IsEnabled = true;
            cmdToggle.Content = SelectedMod.IsActive ? "Deactivate" : "Activate";
            cmdDiff.IsEnabled = false;

            ReloadSourceFileList();
        }

        private void UpdateRightClickMenuOptions()
        {
            CtxRight.Items.Clear();

            // Toggle
            var toggle = new MenuItem();
            toggle.Header = "Toggle";
            toggle.Click += Toggle_Click;
            CtxRight.Items.Add(toggle);

            // Uninstall
            var uninstall = new MenuItem();
            uninstall.Header = "Uninstall";
            uninstall.Click += MenuUninstall_Click;
            CtxRight.Items.Add(uninstall);

            // Set to Version
            var catalogEntry = Catalog.Mods.Find(m => m.Id == SelectedMod!.Id);
            if (catalogEntry != null)
            {
                var versions = catalogEntry.Versions.Keys.ToList();
                versions.ForEach(v =>
                {
                    var update = new MenuItem();
                    update.Header = "Install v" + v;
                    update.Click += (sender, e) => UpdateMod(SelectedMod!, catalogEntry.Versions[v]);
                    update.IsEnabled = SelectedMod!.Meta.GetVersion().ToString() != v;
                    CtxRight.Items.Add(update);
                });
            }
        }

        #region Drag Elements
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
        #endregion

        #region Center Pane
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
            ReloadCatalog();
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
        #endregion

        #region Alert
        public void CriticalAlertPopup(string title, Exception ex)
        {
            CriticalAlertPopup(title, ex.Message + "\n" + ex.StackTrace);
        }

        private void CriticalAlertPopup(string title, string message)
        {
            PrintAsync(message);
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        #endregion

        #region Right Pane
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
        #endregion

        #region App Config Settings
        /// <summary>
        /// Prepopulate a config setting field.
        /// </summary>
        /// <param name="txt"></param>
        /// <param name="key"></param>
        private void PopulateConfigField(TextBox txt, string key, string defaultStr)
        {
            string? dir = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrEmpty(dir))
            {
                dir = defaultStr;
            }
            txt.Text = dir;
            if (!Directory.Exists(dir))
            {
                txt.Background = Invalid;
                Tabs.SelectedItem = TabSettings;
            }
        }

        /// <summary>
        /// Prepopulates a combobox from the config.
        /// </summary>
        /// <param name="cbx"></param>
        /// <param name="key"></param>
        private void PopulateCbx(ComboBox cbx, string key)
        {
            var val = ConfigurationManager.AppSettings[key]!;
            foreach (var item in cbx.Items)
            {
                var optVal = ((ComboBoxItem)item).Content.ToString()!.ToLower();
                if (val == optVal)
                {
                    cbx.SelectedItem = item;
                    return;
                }
            }
        }

        private void TxtSinsDir_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetTxtConfigOption("sinsDir", (TextBox)sender);
        }

        private void TxtModDir_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetTxtConfigOption("modDir", (TextBox)sender);
        }

        private void TxtDownloadDir_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetTxtConfigOption("downDir", (TextBox)sender);
        }

        private void SetTxtConfigOption(string key, TextBox txt)
        {
            string newVal = txt.Text.Replace("/", "\\");
            var exists = Directory.Exists(newVal);
            txt.Background = exists ? Valid : Invalid;

            if (!exists)
            {
                InvalidSettings.Add(key);
            }
            else
            {
                InvalidSettings.Remove(key);
            }
            TabMods.IsEnabled = !InvalidSettings.Any();

            if (exists && newVal != ConfigurationManager.AppSettings[key])
            {
                SetConfigOptions(key, newVal);
            }
        }

        private void CbxChannel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (ComboBoxItem)((ComboBox)sender).SelectedItem;
            SetConfigOptions("channel", item.Content.ToString()!.ToLower());
            ReloadCatalog();
        }

        private void SetConfigOptions(string key, string value)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings[key].Value = value;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
        #endregion

        private void Tabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var control = ((TabControl)sender);
            var selection = (TabItem)control.SelectedItem;

            if (selection != SelectedTab)
            {
                PrintSync($"Tabs_SelectionChanged()");
                if (selection == TabMods)
                {
                    // We have to set this BEFORE the file I/O so we don't start trying to load a second time while still in the first one.
                    // (This can happen if we focus an element and then switch tabs twice.)
                    SelectedTab = selection;
                    ReloadModListFromDisk();
                }
                else
                {
                    SelectedTab = selection;
                }
            }
        }

        #region Play
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
        #endregion

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

        public void PrintSync(Exception ex)
        {
            PrintSync(ex.Message + "\n" + ex.StackTrace);
        }

        /// <summary>
        /// Invoke the print function when possible.
        /// </summary>
        /// <param name="str"></param>
        public void PrintAsync(string str)
        {
            Dispatcher.Invoke(() => PrintSync(str));
        }

        /// <summary>
        /// Invoke the print function when possible.
        /// </summary>
        /// <param name="str"></param>
        public void PrintAsync(Exception ex)
        {
            Dispatcher.Invoke(() => PrintSync(ex));
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

        private void CmdOnline_Click(object sender, RoutedEventArgs e)
        {
            var onlinePopup = new OnlineWindow(Catalog, this);
            onlinePopup.ShowDialog();
        }

        private void MenuUninstall_Click(object sender, RoutedEventArgs e)
        {
            ModManager.Uninstall(this, new Controls.WarningPopup(), Mods, SelectedMod!.Id);
        }

        public bool IsModInstalled(string id)
        {
            return Mods.Any(m => m.Id == id);
        }

        public Dictionary<string, Version> GetModVersions()
        {
            var dict = new Dictionary<string, Version>();
            foreach (var mod in Mods)
            {
                dict.Add(mod.Id, mod.Meta.Version);
            }
            return dict;
        }

        private void UpdateMod(Mod modToUpdate, VersionEntry targetVersion)
        {
            _ = UpdateModInternal(modToUpdate, targetVersion);
        }

        private async Task UpdateModInternal(Mod modToUpdate, VersionEntry targetVersion)
        {
            PrintSync("MenuUpdate_Click()");

            // Delete existing
            ModManager.Uninstall(this, new Controls.WarningPopup(), Mods, modToUpdate.Id, true);

            // Redownload
            var catalogEntry = Catalog.Mods.Find(m => m.Id == SelectedMod!.Id)!;
            await ModManager.InstallModFromGitHub(this, new Controls.WarningPopup(), Catalog, catalogEntry, targetVersion);

            // Reload Mods
            ReloadModListFromDiskAsync();
        }
    }
}
