using Greed.Controls;
using Greed.Controls.Diff;
using Greed.Controls.Online;
using Greed.Controls.Popups;
using Greed.Extensions;
using Greed.Models;
using Greed.Models.Json;
using Greed.Models.ListItem;
using Greed.Models.Online;
using Greed.Models.Vault;
using Microsoft.VisualBasic;
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
using System.Windows.Threading;
using Greed.Exceptions;

namespace Greed
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow? Instance { get; private set; }
        private static readonly string LogPath = Directory.GetCurrentDirectory() + "\\log.txt";
        private static readonly string LogPrevPath = Directory.GetCurrentDirectory() + "\\log_prev.txt";
        private static readonly WarningPopup Warning = new();

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
        private readonly GreedVault Vault = GreedVault.Load();

        public MainWindow()
        {
            Instance = this;
            InitializeComponent();
            SetTitle();
            ReloadCatalog();
            RefreshVaultPackUI();
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
            PopulateConfigField(txtExportDir, "exportDir", Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "Local", "sins2", "mods"));
            PopulateConfigField(txtSinsDir, "sinsDir", "C:\\Program Files\\Epic Games\\SinsII");
            PopulateConfigField(txtDownloadDir, "downDir", Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"));
            PopulateConfigCbx(CbxChannel, "channel");
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
                    ? $" - {Constants.UNI_WARN} Greed v{Catalog.LatestGreed} is now available! {Constants.UNI_WARN}"
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
                Mods = Manager.LoadGreedyMods(Vault);
            }
            catch (Exception e)
            {
                CriticalAlertPopup.Throw("Mod Load Error", new ModLoadException("Unable to load mod(s)", e));
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
                return;
            }
            var item = (ModListItem)e.AddedItems[0]!;
            var newSelection = Mods.First(p => p.Id == item.Id);

            // Ignore the user re-clicking.
            if (newSelection == SelectedMod)
            {
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
                CriticalAlertPopup.Throw("Metadata Error", ex);
            }

            UpdateRightClickMenuOptions();

            // It starts disabled since nothing is selected.
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
            Manager.MoveMod(Vault, Mods, DragMod, Mods.IndexOf(DestMod));
            DragMod = null;
            DestMod = null;
            Vault.ArchiveActiveOnly(Mods);
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
            ToggleSingleMod(SelectedMod);
        }

        private void ToggleSingleMod(Mod? mod)
        {
            PrintSync($"ToggleSingleMod({mod?.Id})");
            if (mod == null) return;

            mod!.SetModActivity(Mods, !mod.IsActive);

            try
            {
                Vault.ArchiveActiveOnly(Mods);
            }
            catch (Exception ex)
            {
                CriticalAlertPopup.Throw("Mod Set Error", new ModLoadException("Unable to locate all files.", ex));
                return;
            }
            RefreshModListUI();
            ReselectSelection();
            ClearCbxBundlesIfMismatch();
        }

        private void ToggleAll_Click(object sender, RoutedEventArgs e)
        {
            PrintSync("ToggleAll_Click()");
            var areAllActive = Mods.All(m => m.IsActive);
            Mods.ForEach(m => m.SetModActivity(Mods, !areAllActive));

            string modDir = ConfigurationManager.AppSettings["modDir"]!;
            try
            {
                Vault.ArchiveActiveOnly(Mods);
            }
            catch (Exception ex)
            {
                CriticalAlertPopup.Throw("Mod Set Error", new ModLoadException("Unable to locate all files.", ex));
                return;
            }
            RefreshModListUI();
            ReselectSelection();
            ClearCbxBundlesIfMismatch();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            PrintSync("Refresh_Click()");
            ReloadModListFromDisk();
            ReloadCatalog();
            PopRefresh.SetPopDuration(2000);
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            ExportActive();
        }

        private void ExportActive()
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
                        _ = PrintAsync("Export Complete");

                        var modDir = ConfigurationManager.AppSettings["modDir"];
                        var expDir = ConfigurationManager.AppSettings["exportDir"];
                        if (modDir != expDir)
                        {
                            var modGreedFolder = modDir! + "\\greed";
                            var expGreedFolder = expDir! + "\\greed";
                            _ = PrintAsync("Copying export to " + expDir + "\\greed");
                            if (Directory.Exists(expDir + "\\greed"))
                            {
                                Directory.Delete(expDir + "\\greed", true);
                            }
                            Extensions.Extensions.CopyDirectory(modGreedFolder, expGreedFolder, true);
                        }

                        var response = MessageBox.Show("Greedy mods are now active. Would you like to start sinning?", "Export Success", MessageBoxButton.YesNo, MessageBoxImage.Information);

                        if (response == MessageBoxResult.Yes)
                        {
                            Play();
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                CriticalAlertPopup.Throw("Failed to Export", ex);
            }
            finally
            {
                cmdExport.Dispatcher.Invoke(() =>
                {
                    cmdExport.IsEnabled = true;
                });
            }
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.F5)
            {
                ReloadModListFromDisk();
                ReloadCatalog();
            }
            else if (e.Key == System.Windows.Input.Key.F6)
            {
                ExportActive();
            }
            else if (e.Key == System.Windows.Input.Key.Space)
            {
                ToggleSingleMod(SelectedMod);
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
                    CriticalAlertPopup.Throw("Failed to Load Mod", ex);
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
                CriticalAlertPopup.Throw("Failed to Load Diff", new DiffException("Failed to load file difference.", ex));
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
        private static void PopulateConfigCbx(ComboBox cbx, string key)
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

        private void TxtExportDir_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetTxtConfigOption("exportDir", (TextBox)sender);
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

        private static void SetConfigOptions(string key, string value)
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
            _ = PrintAsync("Executing: " + execPath);
            Process.Start(new ProcessStartInfo(execPath)
            {
                WorkingDirectory = ConfigurationManager.AppSettings["sinsDir"]!
            });
        }
        #endregion

        #region Printing
        /// <summary>
        /// Sets the progress bar from any thread.
        /// </summary>
        /// <param name="value">[0,1]</param>
        public async Task SetProgressAsync(double value)
        {
            pgbProgress.Dispatcher.Invoke(() =>
            {
                pgbProgress.Value = 100 * value;
            });
            await Task.Delay(0).ConfigureAwait(false);
        }

        /// <summary>
        /// Print synchronously.
        /// </summary>
        /// <param name="str"></param>
        public void PrintSync(string str, string type = "[INFO]")
        {
            var timePrefix = DateTime.Now.ToString("[yyyy/MM/dd | hh:mm:ss.fff] ");
            var prefixed = timePrefix + type + " " + str;
            TxtLog.Text = TxtLog.Text.Any()
                ? TxtLog.Text + '\n' + prefixed
                : prefixed;
            TxtLog.ScrollToEnd();
            File.AppendAllText(LogPath, prefixed + Environment.NewLine);
            Debug.WriteLine(prefixed);
        }

        public void PrintSync(Exception ex)
        {
            PrintSync(ex.Message + "\n" + ex.StackTrace, "[ERROR]");
        }

        public void PrintSync(Exception ex, string type = "[ERROR]")
        {
            PrintSync(ex.Message + "\n" + ex.StackTrace, type);
        }

        /// <summary>
        /// Invoke the print function when possible.
        /// </summary>
        /// <param name="str"></param>
        public async Task PrintAsync(string str, string type = "[INFO]")
        {
            Dispatcher.Invoke(() => PrintSync(str, type));
            await Task.Delay(0).ConfigureAwait(false);
        }

        /// <summary>
        /// Invoke the print function when possible.
        /// </summary>
        /// <param name="str"></param>
        public async Task PrintAsync(Exception ex)
        {
            Dispatcher.Invoke(() => PrintSync(ex));
            await Task.Delay(0).ConfigureAwait(false);
        }

        /// <summary>
        /// Invoke the print function when possible.
        /// </summary>
        /// <param name="str"></param>
        public async Task PrintAsync(Exception ex, string type = "[ERROR]")
        {
            Dispatcher.Invoke(() => PrintSync(ex, type));
            await Task.Delay(0).ConfigureAwait(false);
        }
        #endregion

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

        public void Uninstall(string id)
        {
            ModManager.Uninstall(this, Warning, Mods, id);
        }

        private void MenuUninstall_Click(object sender, RoutedEventArgs e)
        {
            ModManager.Uninstall(this, Warning, Mods, SelectedMod!.Id);
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

        public void UpdateModAsync(Mod modToUpdate, VersionEntry targetVersion)
        {
            Dispatcher.Invoke(() => UpdateModInternal(modToUpdate, targetVersion));
        }

        public async Task UpdateModAsync(OnlineMod onlineModToUpdate, VersionEntry targetVersion)
        {
            var localMod = Mods.Find(m => m.Id == onlineModToUpdate.Id);
            if (localMod == null)
            {
                PrintSync("UpdateModAsync() Abort: unable to update missing local mod " + onlineModToUpdate.Id);
                return;
            }
            await UpdateModInternal(localMod!, targetVersion);
            PrintSync("UpdateModAsync() Complete");
        }

        private void UpdateMod(Mod modToUpdate, VersionEntry targetVersion)
        {
            _ = UpdateModInternal(modToUpdate, targetVersion);
        }

        private async Task UpdateModInternal(Mod modToUpdate, VersionEntry targetVersion)
        {
            PrintSync("MenuUpdate_Click()");

            // Delete existing
            ModManager.Uninstall(this, Warning, Mods, modToUpdate.Id, true);

            // Redownload
            var catalogEntry = Catalog.Mods.Find(m => m.Id == modToUpdate.Id)!;
            await ModManager.InstallModFromInternet(this, Warning, Catalog, catalogEntry, targetVersion);

            // Reload Mods
            ReloadModListFromDiskAsync();
        }

        #region Mod Packs
        private void RefreshVaultPackUI()
        {
            CbxBundles.Items.Clear();
            foreach (var pack in Vault.Packs)
            {
                CbxBundles.Items.Add(pack.Key);
            }
        }

        private void CmdSaveBundle_Click(object sender, RoutedEventArgs e)
        {
            PrintSync("CmdSaveBundle_Click()");
            var name = Interaction.InputBox("Please name the pack. Reusing a name will overwrite the old pack.", "Create Mod Pack");

            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            if (name.Contains('"') || name.Contains('\\'))
            {
                MessageBox.Show("Pack Creation Error", "You cannot create a mod pack with a name containing quotes or escape characters.");
                return;
            }

            Vault.UpsertPack(name, Mods);
            PrintSync("Vault upsert complete.");
            RefreshVaultPackUI();
        }

        private void CmdDeleteBundle_Click(object sender, RoutedEventArgs e)
        {
            PrintSync("CmdDeleteBundle_Click()");
            if (CbxBundles.SelectedItem == null)
            {
                PrintSync("Please select a mod pack.");
                return;
            }

            Vault.DeletePack((string)CbxBundles.SelectedItem);
            PrintSync("Pack deleted.");
            RefreshVaultPackUI();
            CbxBundles.SelectedItem = null;
        }

        private void CbxBundles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PrintSync("CbxBundles_SelectionChanged()");
            if (CbxBundles.SelectedItem == null) return;

            var activePack = Vault.Packs[(string)CbxBundles.SelectedItem];
            Mods.ForEach(m => m.SetModActivity(Mods, activePack.Contains(m.Id), true));
            Vault.ArchiveActiveOnly(Mods);
            PrintSync("Vault archive of active complete.");
            RefreshModListUI();
        }

        /// <summary>
        /// If the selected mods no longer match what was chosen from the dropdown, clear the dropdown.
        /// </summary>
        private void ClearCbxBundlesIfMismatch()
        {
            if (CbxBundles.SelectedItem == null) return;

            var activePack = Vault.Packs[(string)CbxBundles.SelectedItem];

            var actualActive = Mods.Where(m => m.IsActive).Select(m => m.Id).ToList();

            if (activePack.Count != actualActive.Count || activePack.Any(ap => !actualActive.Contains(ap)) || actualActive.Any(aa => !activePack.Contains(aa)))
            {
                CbxBundles.SelectedItem = null;
                PrintSync("Cleared CbxBundles selection.");
            }
        }

        private void CmdCopyBundle_Click(object sender, RoutedEventArgs e)
        {
            PrintSync("CmdCopyBundle_Click()");
            if (CbxBundles.SelectedItem == null)
            {
                PrintSync("Please select a mod pack.");
                return;
            }

            var json = Vault.ExportPortable((string)CbxBundles.SelectedItem, Mods);
            Clipboard.SetText(json);
            PopClipboard.SetPopDuration(2000);
            PrintSync("Copied json to clipboard.");
        }

        private async void CmdImportBundle_Click(object sender, RoutedEventArgs e)
        {
            PrintSync("CmdImportBundle_Click()");
            var json = Clipboard.GetText();
            if (string.IsNullOrEmpty(json))
            {
                PrintSync("Clipboard was empty.", "[WARN]");
                return;
            }

            var portable = PortableVault.Load(json);
            var portableModsToInstall = portable.Mods
                .Where(p => !Mods.Any(m => m.Id == p.Id))
                .ToList();
            var allSuccess = true;
            await portableModsToInstall.ForEachAsync(async portableMod =>
            {
                var modToInstall = Catalog.Mods.Find(m => m.Id == portableMod.Id);
                if (modToInstall == null)
                {
                    MessageBox.Show("Missing Mod", $"The mod {portableMod.Id} could not be located in the online catalog.");
                    allSuccess = false;
                    return;
                }
                var versionToInstall = modToInstall.Versions[portableMod.Version.ToString()];
                var isSuccess = await ModManager.InstallModFromInternet(this, Warning, Catalog, modToInstall, versionToInstall, true);
                if (!isSuccess)
                {
                    allSuccess = false;
                }
            });

            if (!allSuccess)
            {
                await PrintAsync("Failed to install all mods in the mod pack.", "[ERROR]");
                return;
            }

            // Update the Vault
            Vault.ImportPortable(portable);

            // Update the UI and report success.
            if (portableModsToInstall.Any()) ReloadModListFromDisk();
            RefreshVaultPackUI();
            PopImportPack.SetPopDuration(2000);
        }

        #endregion

        private void Window_DragEnter(object sender, DragEventArgs e)
        {
            PrintSync("Window_DragEnter()");
            //if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effects = DragDropEffects.Copy;

            // Changes the icon of the mouse
            e.Effects = DragDropEffects.All;
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            PrintSync("Window_Drop()");
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files) Console.WriteLine(file);
        }

        private void Window_DragLeave(object sender, DragEventArgs e)
        {
            PrintSync("Window_DragLeave()");
        }

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            PrintSync("Window_DragOver()");
        }

        private void viewModList_DragEnter(object sender, DragEventArgs e)
        {
            PrintSync("viewModList_DragEnter()");
        }

        private void viewModList_DragLeave(object sender, DragEventArgs e)
        {
            PrintSync("viewModList_DragLeave()");
        }

        private void viewModList_DragOver(object sender, DragEventArgs e)
        {
            PrintSync("viewModList_DragOver()");
        }

        private void viewModList_Drop(object sender, DragEventArgs e)
        {
            PrintSync("viewModList_Drop()");
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // Process the dropped files here
                _ = InstallDroppedMods(files);
            }
        }

        /// <summary>
        /// Installs the list of mods.
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        private async Task InstallDroppedMods(string[] files)
        {
            await SetProgressAsync(0);
            foreach (string file in files)
            {
                await SetProgressAsync(0.1);
                PrintSync(file);
                await ModManager.InstallMod(this, file);
                await SetProgressAsync(0.95);
            }
            await SetProgressAsync(1);
            ReloadModListFromDiskAsync();
        }
    }
}
