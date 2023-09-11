using Greed.Models.ListItem;
using Greed.Models.Online;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System;

namespace Greed.Controls.Online
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class OnlineWindow : Window
    {
        public WarningPopup Warning { get; set; }
        public OnlineCatalog InstallChannel { get; set; }
        public MainWindow ParentWindow { get; set; }
        private OnlineMod? SelectedMod { get; set; }

        private string SearchQuery { get; set; } = string.Empty;
        private bool SearchUninstalled { get; set; } = false;

        public OnlineWindow(OnlineCatalog listing, MainWindow parent)
        {
            InitializeComponent();
            Debug.WriteLine("OnlineWindow()");
            InstallChannel = listing;
            ParentWindow = parent;

            RefreshOnlineModListUI();
            Warning = new WarningPopup();
        }

        private void RefreshOnlineModListUI()
        {
            ParentWindow.PrintAsync($"RefreshOnlineModListUI for {InstallChannel.Mods.Count} mods.");
            var modVersions = ParentWindow.GetModVersions();
            ViewOnlineModList.Items.Clear();
            InstallChannel.Mods
                .Where(m => string.IsNullOrWhiteSpace(SearchQuery)
                    || m.Id.Contains(SearchQuery)
                    || m.Name.Contains(SearchQuery)
                    || m.Author.Contains(SearchQuery)
                    || m.Description.Contains(SearchQuery))
                .Where(m => !SearchUninstalled || !ParentWindow.IsModInstalled(m.Id))
                .ToList()
                .ForEach(m => ViewOnlineModList.Items.Add(new CatalogListItem(m, modVersions)));
        }

        private void OnlineModList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // This gets hit when refreshing the display.
            if (e.AddedItems.Count == 0)
            {
                return;
            }
            var item = (CatalogListItem)e.AddedItems[0]!;
            var meta = InstallChannel.Mods.First(m => m.Name == item.Name);
            SelectedMod = meta;
            MenuInstall.IsEnabled = !ModManager.IsModInstalled(SelectedMod.Id) && SelectedMod.Versions.Any() && !string.IsNullOrEmpty(SelectedMod.Live.Download);
            try
            {
                TxtOnlineInfo.SetContent(meta, null);
            }
            catch (Exception ex)
            {
                ParentWindow.PrintAsync(ex);
            }
        }

        private void MenuInstall_Click(object sender, RoutedEventArgs e)
        {
            if (ModManager.IsModInstalled(SelectedMod!.Id))
            {
                ParentWindow.PrintAsync("Mod is already installed!");
                return;
            }
            _ = DownloadSelection();
        }

        private async Task DownloadSelection()
        {
            _ = await ModManager.InstallModFromGitHub(ParentWindow, new WarningPopup(), InstallChannel, SelectedMod!, SelectedMod!.Live);
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
