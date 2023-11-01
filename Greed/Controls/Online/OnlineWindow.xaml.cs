using Greed.Models.ListItem;
using Greed.Models.Online;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Greed.Controls.Online
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class OnlineWindow : Window
    {
        public WarningPopup Warning { get; set; }
        public OnlineCatalog Catalog { get; set; }
        public MainWindow ParentWindow { get; set; }
        private OnlineMod? SelectedMod { get; set; }

        private string SearchQuery { get; set; } = string.Empty;
        private bool SearchUninstalled { get; set; } = false;

        private int SelectedIndex = -1;

        public OnlineWindow(OnlineCatalog listing, MainWindow parent)
        {
            InitializeComponent();
            Title = $"Online Catalog of Greedy Mods (Channel: {Greed.Utils.Settings.GetChannel()})";
            Log.Info("OnlineWindow()");
            Catalog = listing;
            ParentWindow = parent;

            RefreshOnlineModListUI();
            Warning = new WarningPopup();
        }

        private void RefreshOnlineModListUI()
        {
            Log.Info($"RefreshOnlineModListUI for {Catalog.Mods.Count} mods.");
            var modVersions = ParentWindow.GetModVersions();
            ViewOnlineModList.Items.Clear();
            var viewableMods = Catalog.Mods
                .Where(m => string.IsNullOrWhiteSpace(SearchQuery)
                    || m.Id.Contains(SearchQuery, StringComparison.InvariantCultureIgnoreCase)
                    || m.Name.Contains(SearchQuery, StringComparison.InvariantCultureIgnoreCase)
                    || m.Author.Contains(SearchQuery, StringComparison.InvariantCultureIgnoreCase)
                    || m.Description.Contains(SearchQuery, StringComparison.InvariantCultureIgnoreCase))
                .Where(m => !SearchUninstalled || !ParentWindow.IsModInstalled(m.Id))
                .OrderByDescending(m => m.Live.DateAdded)
                .ToList();

            for (var i = 0; i < viewableMods.Count; i++)
            {
                ViewOnlineModList.Items.Add(new CatalogListItem(viewableMods[i], modVersions, i % 2 == 0, i == SelectedIndex));
            }
            UpdateRightClickMenuOptions();
        }

        private void OnlineModList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // This gets hit when refreshing the display.
            if (e.AddedItems.Count == 0)
            {
                return;
            }
            var item = (CatalogListItem)e.AddedItems[0]!;
            var meta = Catalog.Mods.First(m => m.Name == item.Name);
            SelectedMod = meta;
            if (SelectedIndex != ViewOnlineModList.SelectedIndex)
            {
                SelectedIndex = ViewOnlineModList.SelectedIndex;
                RefreshOnlineModListUI();
            }
            else
            {
                UpdateRightClickMenuOptions();
            }

            try
            {
                TxtOnlineInfo.SetContent(meta, null);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
        private void UpdateRightClickMenuOptions()
        {
            if (SelectedMod == null)
            {
                return;
            }

            CtxRight.Items.Clear();
            var installedVersions = ParentWindow.GetModVersions();
            var isInstalled = installedVersions.ContainsKey(SelectedMod!.Id);
            var latest = SelectedMod!.GetVersion().ToString();
            var installed = installedVersions.ContainsKey(SelectedMod!.Id) ? installedVersions[SelectedMod!.Id].ToString() : "";

            // Uninstall
            var uninstall = new MenuItem
            {
                Header = "Uninstall"
            };
            uninstall.Click += (sender, e) =>
            {
                ParentWindow.Uninstall(SelectedMod!.Id);
                ParentWindow.ReloadModListFromDiskAsync();
                RefreshOnlineModListUI();
            };
            uninstall.IsEnabled = isInstalled;
            CtxRight.Items.Add(uninstall);

            // Install Live
            var installLive = new MenuItem
            {
                Header = "Install Latest"
            };
            installLive.Click += async (sender, e) => await DownloadSelection(SelectedMod!.Live);
            installLive.IsEnabled = (!isInstalled || installed != latest) && !string.IsNullOrEmpty(SelectedMod.Live.Download);
            CtxRight.Items.Add(installLive);

            CtxRight.Items.Add(new Separator());

            // Install Particular Version
            var catalogEntry = Catalog.Mods.Find(m => m.Id == SelectedMod!.Id);
            if (catalogEntry != null)
            {
                var versions = catalogEntry.Versions.Keys.OrderByDescending(k => k).ToList();
                versions.ForEach(v =>
                {
                    var update = new MenuItem
                    {
                        Header = "Install v" + v
                    };
                    if (isInstalled)
                    {
                        update.Click += async (sender, e) =>
                        {
                            Log.Info("Ctx Click " + (string)update.Header);
                            await ParentWindow.UpdateModAsync(SelectedMod!, catalogEntry.Versions[v]);
                            RefreshOnlineModListUI();
                        };
                        update.IsEnabled = v != installed;
                    }
                    else
                    {
                        update.Click += async (sender, e) =>
                        {
                            Log.Info("Ctx Click " + (string)update.Header);
                            await DownloadSelection(SelectedMod.GetVersionEntry(v));
                        };
                        update.IsEnabled = true;
                    }
                    CtxRight.Items.Add(update);
                });
            }
        }

        private async Task DownloadSelection(VersionEntry versionToInstall)
        {
            _ = await ModManager.InstallModFromInternet(ParentWindow, new WarningPopup(), Catalog, SelectedMod!, versionToInstall);
            ParentWindow.ReloadModListFromDiskAsync();
            RefreshOnlineModListUI();// Do this second because filtration checks what's been loaded by the previous method.
        }

        private void TxtSearchOnline_TextChanged(object sender, TextChangedEventArgs e)
        {
            var txt = (TextBox)sender;
            SearchQuery = txt.Text;
            RefreshOnlineModListUI();
        }

        private void CheckUninstalled_Toggle(object sender, RoutedEventArgs e)
        {
            var cbx = (CheckBox)sender;
            SearchUninstalled = cbx.IsChecked ?? false;
            RefreshOnlineModListUI();
        }
    }
}
