using Greed.Controls;
using Greed.Controls.Diff;
using Greed.Controls.Online;
using Greed.Controls.Popups;
using Greed.Exceptions;
using Greed.Extensions;
using Greed.Models;
using Greed.Models.Json;
using Greed.Models.ListItem;
using Greed.Models.Online;
using Greed.Models.Vault;
using Greed.Utils;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Greed
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow? Instance { get; private set; }
        private static readonly WarningPopup Warning = new();

        private readonly ModManager Manager = new();
        private List<Mod> Mods = new();
        private readonly List<JsonSource> AllSources = new();
        private JsonSource? SelectedSource;

        private readonly SolidColorBrush Valid = new(Colors.White);
        private readonly SolidColorBrush Invalid = new(Colors.Pink);

        private bool ReadyToDrag = false;
        private Mod? DragMod;
        private Mod? DestMod;
        private Mod? DragPastMod;
        private int ActualSelectedModIndex = -1;
        private string SearchQuery = string.Empty;
        private bool SearchActive = false;
        private OnlineCatalog Catalog = new();
        private TabItem? SelectedTab = null;
        private readonly HashSet<string> InvalidSettings = new();
        private readonly GreedVault Vault = GreedVault.Load();

        private CancellationTokenSource? ManualLinkCancellationTokenSource = null;
        public List<Slider> ScalarSliders = new();

        public MainWindow()
        {
            Instance = this;
            InitializeComponent();
            SetTitle();
            ReloadCatalog();
            LoadSettings();

            RefreshVaultPackUI();
            Log.Info("Components Loaded");

            TxtLocalModInfo.Document.Blocks.Clear();
            TxtLocalModInfo.AppendText("Select a mod to view details about it.");

            // Only load mods if there's something to load.
            if (SelectedTab != TabSettings)
            {
                ReloadModListFromDisk();
            }

            pgbProgress.Value = 0;// Ranges [0, 100].
        }

        private void SetTitle()
        {
            string modDir = Settings.GetModDir();
            string sinsDir = Settings.GetSinsDir();
            Log.Info($"Mod Dir: {modDir}");
            Log.Info($"Sins II Dir: {sinsDir}");
            try
            {
                var greedVersion = Settings.GetGreedVersion();
                var updateStr = greedVersion.IsOlderThan(Catalog.LatestGreed)
                    ? $" - {Utils.Constants.UNI_WARN} Greed v{Catalog.LatestGreed} is now available! {Utils.Constants.UNI_WARN}"
                    : "";
                Title = $"Greed Mod Loader v{greedVersion} (Detected Sins II v{Settings.GetSinsVersion()}){updateStr}";
            }
            catch (Exception)
            {
                ViewModList.Items.Clear();
                Tabs.SelectedItem = TabSettings;
            }
        }

        private void SetTitleAsync()
        {
            Dispatcher.Invoke(() => SetTitle());
        }

        #region Left Pane
        #region Reload Mods
        /// <summary>
        /// Reloads the list of installed mods
        /// </summary>
        private void ReloadModListFromDisk()
        {
            Log.Info("Loading Settings...");
            Log.Info("Loading mods...");
            try
            {
                Mods = Manager.LoadGreedyMods(Vault);
            }
            catch (Exception e)
            {
                CriticalAlertPopup.Throw("Mod Load Error", new ModLoadException("Unable to load mod(s)", e));
                ViewModList.Items.Clear();
                Tabs.SelectedItem = TabSettings;
                return;
            }
            Log.Info("Load succeeded.");

            RefreshModListUI();
            ReloadSourceFileList();
            Log.Info("Refresh Complete");
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
            Log.Info($"Refreshing mod list UI for {Mods.Count} mods...");
            ViewModList.Items.Clear();
            var printableMods = Mods
                .Where(m => string.IsNullOrWhiteSpace(SearchQuery)
                    || m.Id.Contains(SearchQuery, StringComparison.InvariantCultureIgnoreCase)
                    || m.Readme.Contains(SearchQuery, StringComparison.InvariantCultureIgnoreCase)
                    || m.Meta.Name.Contains(SearchQuery, StringComparison.InvariantCultureIgnoreCase)
                    || m.Meta.Author.Contains(SearchQuery, StringComparison.InvariantCultureIgnoreCase)
                    || m.Meta.Description.Contains(SearchQuery, StringComparison.InvariantCultureIgnoreCase))
                .Where(m => !SearchActive || m.IsActive)
                .ToList();

            for (var i = 0; i < printableMods.Count; i++)
            {
                var m = printableMods[i];
                ViewModList.Items.Add(new ModListItem(m, Catalog, i % 2 == 0, i == ActualSelectedModIndex));
            }
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
                Catalog = await OnlineCatalog.GetOnlineListing();
                RefreshModListUIAsync();
                SetTitleAsync();
            }).ConfigureAwait(false);
        }
        #endregion

        #region Mod List Interaction
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

        private void Ctx_Toggle_Click(object sender, RoutedEventArgs e)
        {
            ToggleSingleMod(DragPastMod);
        }

        private void Toggle_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ToggleSingleMod(DragPastMod);
        }
        #endregion

        #region Select and Drag Elements
        private void ModList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PrintDrag("ModList_SelectionChanged() from " + DragPastMod);
            // This gets hit when refreshing the display.
            if (e.AddedItems.Count == 0)
            {
                return;
            }
            var item = (ModListItem)e.AddedItems[0]!;
            var newSelection = Mods.First(p => p.Id == item.Id);

            DragPastMod = newSelection;
            Debug.WriteLine("- New Selected Mod " + DragPastMod.ToString());

            // If we're ready to drag, start the drag.
            if (ReadyToDrag || ActualSelectedModIndex == -1)
            {
                // Update the flag so the binding catches which to highlight.
                if (ActualSelectedModIndex != ViewModList.SelectedIndex)
                {
                    // Out with the old
                    if (ActualSelectedModIndex != -1)
                    {
                        ((ModListItem)ViewModList.Items[ActualSelectedModIndex]).IsSelected = false;
                    }
                    // In with the new
                    ActualSelectedModIndex = ViewModList.SelectedIndex;
                    ((ModListItem)ViewModList.Items[ActualSelectedModIndex]).IsSelected = true;
                    RefreshModListUI();
                }

                // Initiate drag.
                DragMod = DragPastMod;
                ReadyToDrag = false;
                Debug.WriteLine("- Start drag from " + DragMod.ToString());
            }
            else
            {
                DestMod = DragPastMod;
                Debug.WriteLine("- Temporary destination mod is " + DestMod.ToString());
            }

            // Load the text field.
            try
            {
                TxtLocalModInfo.SetContent(Mods[ActualSelectedModIndex].Meta, Mods[ActualSelectedModIndex]);
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
            var toggle = new MenuItem
            {
                Header = "Toggle"
            };
            toggle.Click += Ctx_Toggle_Click;
            CtxRight.Items.Add(toggle);

            // Uninstall
            var uninstall = new MenuItem
            {
                Header = "Uninstall"
            };
            uninstall.Click += MenuUninstall_Click;
            CtxRight.Items.Add(uninstall);

            // Set to Version
            var catalogEntry = Catalog.Mods.Find(m => m.Id == DragPastMod!.Id);
            if (catalogEntry != null)
            {
                var versions = catalogEntry.Versions.Keys.ToList();
                versions.ForEach(v =>
                {
                    var update = new MenuItem
                    {
                        Header = "Install v" + v
                    };
                    update.Click += (sender, e) => UpdateMod(DragPastMod!, catalogEntry.Versions[v]);
                    update.IsEnabled = DragPastMod!.Meta.GetVersion().ToString() != v;
                    CtxRight.Items.Add(update);
                });
            }
        }

        private void ModList_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PrintDrag("ModList_MouseUp()");
            if (DestMod == null || DragMod == null || DragMod == DestMod)
            {
                Debug.WriteLine("- Abort drag-and-drop");
                // We aren't or aborted drag-and-drop.
                DragMod = null;
                DestMod = null;
                ReadyToDrag = false;
                return;
            }

            var destIndex = Mods.IndexOf(DestMod);
            try
            {
                Debug.WriteLine("Initiating drag-and-drop movement for " + DragMod?.ToString());

                // We *are* actually reordering now.
                Manager.MoveMod(Vault, Mods, DragMod!, destIndex);
            }
            catch (Exception ex)
            {
                CriticalAlertPopup.Throw("Failed to reorder mod list.", ex);
            }
            DragMod = null;
            DestMod = null;
            Vault.ArchiveActiveOnly(Mods);
            ActualSelectedModIndex = destIndex;
            RefreshModListUI();
        }

        private void ModList_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            PrintDrag("ModList_MouseLeave()");
            DragMod = null;
            DestMod = null;
            ReadyToDrag = false;
        }

        private void ModList_MousePreview(object sender, System.Windows.Input.MouseEventArgs e)
        {
            PrintDrag("ModList_MousePreview()");
            DragMod = null;
            DestMod = null;
            ReadyToDrag = true;
        }

        private void PrintDrag(string method)
        {
            Debug.WriteLine(method);
            Debug.WriteLine("- ASMI: " + ActualSelectedModIndex);
            Debug.WriteLine("- DragMod: " + DragMod);
            Debug.WriteLine("- DragPastMod: " + DragPastMod);
            Debug.WriteLine("- DragDest: " + DestMod);
        }
        #endregion

        #region Column Sorting
        private void HeaderModName_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("HeaderModName_Click()");
        }

        private void HeaderVersion_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("HeaderVersion_Click()");
        }

        private void HeaderGreed_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("HeaderGreed_Click()");
        }

        private void HeaderSins_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("HeaderSins_Click()");
        }
        #endregion

        private void ToggleSingleMod(Mod? mod)
        {
            Log.Debug($"ToggleSingleMod({mod?.Id})");
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
            Log.Debug("ToggleAll_Click()");
            var anyAreActive = Mods.Any(m => m.IsActive);
            Mods.ForEach(m => m.SetModActivity(Mods, !anyAreActive, anyAreActive));

            string modDir = Settings.GetModDir();
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
            Log.Debug("Refresh_Click()");
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

            Log.Info("Exporting...");
            try
            {
                var active = Mods.Where(m => m.IsGreedy && m.IsActive).ToList();
                Manager.ExportGreedyMods(active, pgbProgress, this, (exportSucceeded) =>
                {
                    if (exportSucceeded)
                    {
                        Log.Info("Export Complete");

                        var modDir = Settings.GetModDir();
                        var expDir = Settings.GetExportDir();
                        if (modDir != expDir)
                        {
                            var modGreedFolder = modDir! + "\\greed";
                            var expGreedFolder = expDir! + "\\greed";
                            Log.Info("Copying export to " + expDir + "\\greed");
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

        private void ReselectSelection()
        {
            DragPastMod = Mods.Find(m => m.Id == DragPastMod?.Id);
            if (DragPastMod != null)
            {
                var index = Mods.IndexOf(DragPastMod!);
                ViewModList.SelectedItem = DragPastMod;
                ViewModList.SelectedIndex = index;
            }
        }
        #endregion

        #region Center Pane
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
                ToggleSingleMod(DragPastMod);
            }
        }
        #endregion

        #region Right Pane
        private void ReloadSourceFileList()
        {
            SelectedSource = null;
            viewFileList.Items.Clear();
            // Only populate this if we actually have something selected, and we didn't delete the last mod in the list.
            if (ActualSelectedModIndex > -1 && ActualSelectedModIndex < Mods.Count)
            {
                var selectedMod = Mods[ActualSelectedModIndex];
                AllSources.Clear();

                // Json
                AllSources.AddRange(selectedMod.Brushes);
                AllSources.AddRange(selectedMod.Colors);
                AllSources.AddRange(selectedMod.Cursors);
                AllSources.AddRange(selectedMod.DeathSequences);
                AllSources.AddRange(selectedMod.Effects);
                AllSources.AddRange(selectedMod.Fonts);
                AllSources.AddRange(selectedMod.GravityWellProps);
                AllSources.AddRange(selectedMod.Gui);
                AllSources.AddRange(selectedMod.MeshMaterials);
                AllSources.AddRange(selectedMod.PlayerColors);
                AllSources.AddRange(selectedMod.PlayerIcons);
                AllSources.AddRange(selectedMod.PlayerPortraits);
                AllSources.AddRange(selectedMod.Skyboxes);
                AllSources.AddRange(selectedMod.TextureAnimations);
                AllSources.AddRange(selectedMod.Uniforms);
                AllSources.AddRange(selectedMod.Entities);
                AllSources.AddRange(selectedMod.LocalizedTexts);

                // Non-Json
                //AllSources.AddRange(selectedMod.Meshes);
                //AllSources.AddRange(selectedMod.Scenarios);
                //AllSources.AddRange(selectedMod.Shaders);
                //AllSources.AddRange(selectedMod.Sounds);
                //AllSources.AddRange(selectedMod.Textures);

                try
                {
                    for (var i = 0; i < AllSources.Count; i++)
                    {
                        viewFileList.Items.Add(new SourceListItem(AllSources[i], i % 2 == 0));
                    }
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
            SelectedSource = AllSources.Find(p => p.Filename == item.Filename && p.Folder == item.Folder);
            Log.Debug("Selected " + SelectedSource?.Mergename);
            cmdDiff.IsEnabled = true;
        }

        private void Diff_Click(object sender, RoutedEventArgs e)
        {
            Log.Debug("Diff_Click()");
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

        #region Settings
        private void LoadSettings()
        {
            PopulateConfigField(TxtModsDir, Settings.GetModDir(), Settings.DefaultModDir);
            PopulateConfigField(TxtExportDir, Settings.GetExportDir(), Settings.DefaultModDir);
            PopulateConfigField(TxtSinsDir, Settings.GetSinsDir(), Settings.DefaultSinDir);
            PopulateConfigField(TxtDownloadDir, Settings.GetDownDir(), Settings.DefaultDownDir);

            ExpGlobalScalars.Height = 10 + 50 * Settings.PopulateScalarExpander((Grid)ExpGlobalScalars.Content);
        }

        /// <summary>
        /// Prepopulate a config setting field.
        /// </summary>
        /// <param name="txt"></param>
        /// <param name="key"></param>
        private void PopulateConfigField(TextBox txt, string dir, string defaultStr)
        {
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
            var txt = (TextBox)sender;
            if (SetFolderConfigOption("Sins II Directory", txt))
                Settings.SetSinsDir(txt.Text);
        }

        private void TxtModDir_TextChanged(object sender, TextChangedEventArgs e)
        {
            var txt = (TextBox)sender;
            if (SetFolderConfigOption("Mod Vault Directory", txt))
                Settings.SetModDir(txt.Text);
        }

        private void TxtExportDir_TextChanged(object sender, TextChangedEventArgs e)
        {
            var txt = (TextBox)sender;
            if (SetFolderConfigOption("Mod Export Directory", txt))
                Settings.SetExportDir(txt.Text);
        }

        private void TxtDownloadDir_TextChanged(object sender, TextChangedEventArgs e)
        {
            var txt = (TextBox)sender;
            if (SetFolderConfigOption("Working Download Directory", txt))
                Settings.SetDownDir(txt.Text);
        }

        private bool SetFolderConfigOption(string key, TextBox txt)
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
            return exists;
        }

        private void CbxChannel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (ComboBoxItem)((ComboBox)sender).SelectedItem;
            txtManualCatalog.Clear();
            Settings.SetChannel(item.Content.ToString()!.ToLower());
            ReloadCatalog();
        }

        private void TxtManualCatalog_TextChanged(object sender, TextChangedEventArgs e)
        {
            var txt = (TextBox)sender;
            if (string.IsNullOrEmpty(txt.Text))
            {
                Settings.SetChannel("live");
            }
            CancellationTokenSource? localSource = null;
            ManualLinkCancellationTokenSource?.Cancel();
            ManualLinkCancellationTokenSource = new();
            localSource = ManualLinkCancellationTokenSource;
            var content = txt.Text;

            Task.Run(async () =>
            {
                using var client = new HttpClient();
                try
                {
                    var result = await client.GetAsync(content);
                    var json = await result.Content.ReadAsStringAsync();
                    var listing = JsonConvert.DeserializeObject<OnlineCatalog>(json);
                    if (!localSource.IsCancellationRequested)
                    {
                        PopManualCatalogGood.Dispatcher.Invoke(() => PopManualCatalogGood.SetPopDuration(2000));
                        Settings.SetChannel(content);
                        ReloadCatalog();
                    }
                }
                catch (Exception ex)
                {
                    if (!localSource.IsCancellationRequested)
                    {
                        Log.Warn(ex);
                        PopManualCatalogBad.Dispatcher.Invoke(() => PopManualCatalogBad.SetPopDuration(2000));
                    }
                    else
                    {
                        CriticalAlertPopup.ThrowAsync("Failed to even check if link is valid", ex);
                    }
                }
            });
        }
        private void CmdResetSliders_Click(object sender, RoutedEventArgs e)
        {
            Settings.ResetSliders(GridScalarParent);
        }
        #endregion

        private void CmdUpdateGreed_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(async () =>
            {
                Catalog = await OnlineCatalog.GetOnlineListing();
                if (Assembly.GetExecutingAssembly().GetName().Version!.IsOlderThan(Catalog.LatestGreed))
                {
                    var result = MessageBox.Show("You have an oudated version of Greed. Would you like to update?", "Update Available", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (result == MessageBoxResult.Yes)
                    {
                        await IOManager.UpdateGreed(Catalog.LatestGreed);
                    }
                }
                else
                {
                    MessageBox.Show("You have the latest version of Greed", "No Update", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }).ConfigureAwait(false);
        }

        private void Tabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var control = ((TabControl)sender);
            var selection = (TabItem)control.SelectedItem;

            if (selection != SelectedTab)
            {
                Log.Info($"Tabs_SelectionChanged()");
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
        private static void Play()
        {
            var execPath = Settings.GetSinsExePath();
            Log.Info("Executing: " + execPath);
            Process.Start(new ProcessStartInfo(execPath)
            {
                WorkingDirectory = Settings.GetSinsDir()
            });
        }
        #endregion

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

        private void TxtLog_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Clipboard.SetText(Log.GetLogs());
            PopupCopiedLog.SetPopDuration(2000);
            Log.Info("Copied json to clipboard.");
        }

        private void CmdOnline_Click(object sender, RoutedEventArgs e)
        {
            var onlinePopup = new OnlineWindow(Catalog, this);
            onlinePopup.ShowDialog();
        }

        #region Uninstall Mod
        public void Uninstall(string id)
        {
            ModManager.Uninstall(this, Warning, Mods, id);
        }

        private void MenuUninstall_Click(object sender, RoutedEventArgs e)
        {
            ModManager.Uninstall(this, Warning, Mods, DragPastMod!.Id);
        }

        public bool IsModInstalled(string id)
        {
            return Mods.Any(m => m.Id == id);
        }
        #endregion

        public Dictionary<string, Version> GetModVersions()
        {
            var dict = new Dictionary<string, Version>();
            foreach (var mod in Mods)
            {
                dict.Add(mod.Id, mod.Meta.Version);
            }
            return dict;
        }

        #region Update Mod
        public void UpdateModAsync(Mod modToUpdate, VersionEntry targetVersion)
        {
            Dispatcher.Invoke(() => UpdateModInternal(modToUpdate, targetVersion));
        }

        public async Task UpdateModAsync(OnlineMod onlineModToUpdate, VersionEntry targetVersion)
        {
            var localMod = Mods.Find(m => m.Id == onlineModToUpdate.Id);
            if (localMod == null)
            {
                Log.Warn("UpdateModAsync() Abort: unable to update missing local mod " + onlineModToUpdate.Id);
                return;
            }
            await UpdateModInternal(localMod!, targetVersion);
            Log.Info("UpdateModAsync() Complete");
        }

        private void UpdateMod(Mod modToUpdate, VersionEntry targetVersion)
        {
            _ = UpdateModInternal(modToUpdate, targetVersion);
        }

        private async Task UpdateModInternal(Mod modToUpdate, VersionEntry targetVersion)
        {
            Log.Debug("MenuUpdate_Click()");

            // Delete existing
            ModManager.Uninstall(this, Warning, Mods, modToUpdate.Id, true);

            // Redownload
            var catalogEntry = Catalog.Mods.Find(m => m.Id == modToUpdate.Id)!;
            await ModManager.InstallModFromInternet(this, Warning, Catalog, catalogEntry, targetVersion);

            // Reload Mods
            ReloadModListFromDiskAsync();
        }
        #endregion

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
            Log.Debug("CmdSaveBundle_Click()");
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
            Log.Info("Vault upsert complete.");
            RefreshVaultPackUI();
        }

        private void CmdDeleteBundle_Click(object sender, RoutedEventArgs e)
        {
            Log.Debug("CmdDeleteBundle_Click()");
            if (CbxBundles.SelectedItem == null)
            {
                Log.Warn("Please select a mod pack.");
                return;
            }

            Vault.DeletePack((string)CbxBundles.SelectedItem);
            Log.Info("Pack deleted.");
            RefreshVaultPackUI();
            CbxBundles.SelectedItem = null;
        }

        private void CbxBundles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Log.Debug("CbxBundles_SelectionChanged()");
            if (CbxBundles.SelectedItem == null) return;

            var activePack = Vault.Packs[(string)CbxBundles.SelectedItem];
            Mods.ForEach(m => m.SetModActivity(Mods, activePack.Contains(m.Id), true));
            Vault.ArchiveActiveOnly(Mods);
            Log.Info("Vault archive of active complete.");
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
                Log.Debug("Cleared CbxBundles selection.");
            }
        }

        private void CmdCopyBundle_Click(object sender, RoutedEventArgs e)
        {
            Log.Debug("CmdCopyBundle_Click()");
            if (CbxBundles.SelectedItem == null)
            {
                Log.Warn("Please select a mod pack.");
                return;
            }

            var json = Vault.ExportPortable((string)CbxBundles.SelectedItem, Mods);
            Clipboard.SetText(json);
            PopClipboard.SetPopDuration(2000);
            Log.Info("Copied json to clipboard.");
        }

        private async void CmdImportBundle_Click(object sender, RoutedEventArgs e)
        {
            Log.Debug("CmdImportBundle_Click()");
            var json = Clipboard.GetText();
            if (string.IsNullOrEmpty(json))
            {
                Log.Warn("Clipboard was empty.");
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
                Log.Error("Failed to install all mods in the mod pack.");
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

        #region File Drag-and-Drop
        private void Window_DragEnter(object sender, DragEventArgs e)
        {
            Log.Debug("Window_DragEnter()");

            // Changes the icon of the mouse
            e.Effects = DragDropEffects.All;
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            Log.Debug("Window_Drop()");
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files) Console.WriteLine(file);
        }

        private void ViewModList_Drop(object sender, DragEventArgs e)
        {
            Log.Debug("ViewModList_Drop()");
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
                Log.Info(file);
                await ModManager.InstallMod(this, file);
                await SetProgressAsync(0.95);
            }
            await SetProgressAsync(1);
            ReloadModListFromDiskAsync();
        }
        #endregion

        #region Developer Tab
        private void CmdGenerateCatalog_Click(object sender, RoutedEventArgs e)
        {
            var cataclone = JsonConvert.DeserializeObject<OnlineCatalog>(JsonConvert.SerializeObject(Catalog))!;

            var active = Mods.Where(m => m.IsActive).ToList();

            var upserted = active.Where(local =>
            {
                var online = cataclone.Mods.FirstOrDefault(p => p.Id == local.Id);
                return online == null || online.Latest.IsOlderThan(local.Meta.Version);
            }).ToList();

            if (!upserted.Any())
            {
                MessageBox.Show("No mods were detected that need to be upserted in the catalog.");
                return;
            }

            MessageBox.Show($"Detected the following mods to upsert:\n{string.Join("\n", upserted.Select(p => p.Meta.Name + " v" + p.Meta.Version))}");

            upserted.ForEach(local =>
            {
                var online = cataclone.Mods.FirstOrDefault(p => p.Id == local.Id);
                var ve = new VersionEntry
                {
                    GreedVersion = local.Meta.GreedVersion,
                    SinsVersion = local.Meta.SinsVersion,
                    Conflicts = local.Meta.Conflicts,
                    DateAdded = DateAndTime.Now.ToString("yyyy-MM-dd HH:mm"),
                    Dependencies = local.Meta.Dependencies,
                    Download = ""
                };

                // Check if the mod is new
                if (online == null)
                {
                    var infoUrl = Interaction.InputBox("Please provide the mod's informational URL.", $"New Mod: {local.Meta.Name} v{local.Meta.Version}");
                    if (string.IsNullOrEmpty(infoUrl))
                    {
                        return;
                    }
                    ve.Download = Interaction.InputBox("Please provide the public static download URL.", $"New Mod: {local.Meta.Name} v{local.Meta.Version}");
                    if (string.IsNullOrEmpty(ve.Download))
                    {
                        return;
                    }
                    cataclone.Mods.Add(new OnlineMod
                    {
                        Author = local.Meta.Author,
                        Description = local.Meta.Description,
                        Id = local.Id,
                        IsTotalConversion = local.Meta.IsTotalConversion,
                        Latest = local.Meta.Version,
                        Name = local.Meta.Name,
                        Url = infoUrl,
                        Versions = new()
                        {
                            { local.Meta.Version.ToString(), ve }
                        }
                    });
                }
                // Check if the mod has been updated
                else if (local.Meta.Version.IsNewerThan(online.Latest))
                {
                    ve.Download = Interaction.InputBox("Please provide the public static download URL.", $"Updated Mod: {local.Meta.Name} to {local.Meta.Version}");
                    if (string.IsNullOrEmpty(ve.Download))
                    {
                        return;
                    }
                    online.Versions.Add(local.Meta.Version.ToString(), ve);
                    online.Latest = local.Meta.Version;
                }
            });

            var json = JsonConvert.SerializeObject(cataclone, Formatting.Indented);
            Clipboard.SetText(json);
            PopClipboardCatalog.SetPopDuration(2000);
        }

        private void CmdGreedyWizard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ModManager.NewModWizard(Catalog);
            }
            catch (Exception ex)
            {
                CriticalAlertPopup.Throw("Failed to generate new mod (probably due to bad user input)", ex);
            }
        }

        private void CmdZipActiveMod_Click(object sender, RoutedEventArgs e)
        {
            var active = Mods.Where(a => a.IsActive).ToList();
            if (active.Count != 1)
            {
                MessageBox.Show("Only one mod can be zipped at a time. Please ensure you have exactly one mod set to active.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                var path = IOManager.ZipMod(active[0]);
                Clipboard.SetFileDropList(new System.Collections.Specialized.StringCollection { path });
                PopClipboardZipMod.SetPopDuration(2000);
            }
            catch (Exception ex)
            {
                CriticalAlertPopup.Throw("Failed to zip mod.", ex);
            }
        }
        #endregion
    }
}
