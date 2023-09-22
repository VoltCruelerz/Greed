using Greed.Models.ListItem;
using Greed.Models.Online;
using System;
using System.Diagnostics;
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

        public OnlineWindow(OnlineCatalog listing, MainWindow parent)
        {
            InitializeComponent();
            Debug.WriteLine("OnlineWindow()");
            Catalog = listing;
            ParentWindow = parent;

            RefreshOnlineModListUI();
            Warning = new WarningPopup();
        }

        private void RefreshOnlineModListUI()
        {
            ParentWindow.PrintAsync($"RefreshOnlineModListUI for {Catalog.Mods.Count} mods.");
            var modVersions = ParentWindow.GetModVersions();
            ViewOnlineModList.Items.Clear();
            Catalog.Mods
                .Where(m => string.IsNullOrWhiteSpace(SearchQuery)
                    || m.Id.Contains(SearchQuery)
                    || m.Name.Contains(SearchQuery)
                    || m.Author.Contains(SearchQuery)
                    || m.Description.Contains(SearchQuery))
                .Where(m => !SearchUninstalled || !ParentWindow.IsModInstalled(m.Id))
                .ToList()
                .ForEach(m => ViewOnlineModList.Items.Add(new CatalogListItem(m, modVersions)));
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
            UpdateRightClickMenuOptions();

            try
            {
                TxtOnlineInfo.SetContent(meta, null);
            }
            catch (Exception ex)
            {
                ParentWindow.PrintAsync(ex);
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
            var uninstall = new MenuItem();
            uninstall.Header = "Uninstall";
            uninstall.Click += (sender, e) =>
            {
                ParentWindow.Uninstall(SelectedMod!.Id);
                ParentWindow.ReloadModListFromDiskAsync();
                RefreshOnlineModListUI();
            };
            uninstall.IsEnabled = isInstalled;
            CtxRight.Items.Add(uninstall);

            // Install Live
            var installLive = new MenuItem();
            installLive.Header = "Install Latest";
            installLive.Click += async (sender, e) => await DownloadSelection(SelectedMod!.Live);
            installLive.IsEnabled = (!isInstalled || installed != latest) && !string.IsNullOrEmpty(SelectedMod.Live.Download);
            CtxRight.Items.Add(installLive);

            CtxRight.Items.Add(new Separator());

            // Install Particular Version
            var catalogEntry = Catalog.Mods.Find(m => m.Id == SelectedMod!.Id);
            if (catalogEntry != null)
            {
                var versions = catalogEntry.Versions.Keys.ToList();
                versions.ForEach(v =>
                {
                    var update = new MenuItem();
                    update.Header = "Install v" + v;
                    if (isInstalled)
                    {
                        update.Click += async (sender, e) =>
                        {
                            ParentWindow.PrintSync("Ctx Click " + (string)update.Header);
                            await ParentWindow.UpdateModAsync(SelectedMod!, catalogEntry.Versions[v]);
                            RefreshOnlineModListUI();
                        };
                        update.IsEnabled = v != installed;
                    }
                    else
                    {
                        update.Click += async (sender, e) =>
                        {
                            ParentWindow.PrintSync("Ctx Click " + (string)update.Header);
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
            _ = await ModManager.InstallModFromGitHub(ParentWindow, new WarningPopup(), Catalog, SelectedMod!, versionToInstall);
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
