using Greed.Models.ListItem;
using Greed.Models;
using Greed.Models.Online;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
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
        public OnlineChannel Listing { get; set; }
        public MainWindow ParentWindow { get; set; }
        private OnlineMod? SelectedMod { get; set; }

        private string SearchQuery { get; set; } = string.Empty;
        private bool SearchUninstalled { get; set; } = false;

        public OnlineWindow(OnlineChannel listing, MainWindow parent)
        {
            InitializeComponent();
            Debug.WriteLine("OnlineWindow()");
            Listing = listing;
            ParentWindow = parent;

            RefreshOnlineModListUI();
            Warning = new WarningPopup();
        }

        private void RefreshOnlineModListUI()
        {
            ParentWindow.PrintAsync($"RefreshOnlineModListUI for {Listing.Mods.Count} mods.");
            ViewOnlineModList.Items.Clear();
            Listing.Mods
                .Where(m => string.IsNullOrWhiteSpace(SearchQuery)
                    || m.Id.Contains(SearchQuery)
                    || m.Name.Contains(SearchQuery)
                    || m.Author.Contains(SearchQuery)
                    || m.Description.Contains(SearchQuery))
                .Where(m => !SearchUninstalled || !ParentWindow.IsModInstalled(m.Id))
                .ToList()
                .ForEach(m => ViewOnlineModList.Items.Add(new OnlineListItem(m)));
        }

        private void OnlineModList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // This gets hit when refreshing the display.
            if (e.AddedItems.Count == 0)
            {
                return;
            }
            var item = (OnlineListItem)e.AddedItems[0]!;
            var meta = Listing.Mods.First(m => m.Name == item.Name);
            SelectedMod = meta;
            MenuInstall.IsEnabled = !ModManager.IsModInstalled(SelectedMod.Id) && SelectedMod.Versions.Any();
            TxtOnlineInfo.SetContent(meta, null);
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
            await InstallModFromGitHub(SelectedMod!, SelectedMod!.Live);
            ParentWindow.ReloadModListFromDiskAsync();
            RefreshOnlineModListUI();// Do this second because filtration checks what's been loaded by the previous method.
        }

        private async Task<bool> InstallModFromGitHub(OnlineMod modToDownload, VersionEntry versionToDownload)
        {
            try
            {
                ParentWindow.PrintAsync($"Installing {modToDownload.Name}...");
                var url = versionToDownload.Download;

                if (!await DependenciesReady(modToDownload, versionToDownload))
                {
                    ParentWindow.PrintAsync($"Download of {modToDownload.Name} aborted.");
                    return false;
                }

                // Download the file to the Downloads directory
                var filename = modToDownload.Id + "_" + url.Split('/')[^1];
                var zipPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "Downloads",
                    filename
                );
                if (File.Exists(zipPath))
                {
                    File.Delete(zipPath);
                }
                await DownloadZipFile(url, zipPath);
                ParentWindow.PrintAsync($"Download of {modToDownload.Name} to {zipPath} complete.");

                // Extract the mod.
                var extractPath = zipPath.Split(".zip")[0];
                if (Directory.Exists(extractPath))
                {
                    Directory.Delete(extractPath, true);
                }
                ParentWindow.PrintAsync($"Extracting to {extractPath}...");
                ZipFile.ExtractToDirectory(zipPath, extractPath);
                ParentWindow.PrintAsync($"Extract complete.");

                // Shift to the mod directory
                var internalDir = Directory.GetDirectories(extractPath)[0];
                var modPath = ConfigurationManager.AppSettings["modDir"]! + "\\" + modToDownload!.Id;
                if (Directory.Exists(modPath))
                {
                    Directory.Delete(modPath, true);
                }
                Directory.Move(internalDir, modPath);
                ParentWindow.PrintAsync($"Move complete.");
                ParentWindow.PrintAsync($"Install complete.");

                // Cleanup
                File.Delete(zipPath);
                Directory.Delete(extractPath, true);
                return true;
            }
            catch (Exception ex)
            {
                ParentWindow.PrintAsync(ex.Message + "\n" + ex.StackTrace);
                return false;
            }
        }

        private async Task<bool> DependenciesReady(OnlineMod onlineMod, VersionEntry desiredVersion)
        {
            foreach (var dep in desiredVersion.Dependencies)
            {
                if (dep.IsOutdatedOrMissing())
                {
                    var result = Warning.ChainedInstall(onlineMod, dep);
                    if (result == MessageBoxResult.Cancel)
                    {
                        return false;
                    }
                    else if (result == MessageBoxResult.Yes)
                    {
                        var dependencyMod = Listing.Mods.Find(m => m.Id == dep.Id);
                        var versionToInstall = dependencyMod!.Versions[dep.Version.ToString()];
                        var chainResult = await InstallModFromGitHub(dependencyMod, versionToInstall);
                        if (!chainResult)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        // No means do nothing.
                    }
                }
            }
            return true;
        }

        private async Task DownloadZipFile(string releaseUrl, string outputPath)
        {
            using HttpClient httpClient = new();
            try
            {
                // Send an HTTP GET request to the GitHub release URL
                HttpResponseMessage response = await httpClient.GetAsync(releaseUrl);

                // Check if the request was successful (HTTP status code 200)
                if (response.IsSuccessStatusCode)
                {
                    // Get the response stream and create a FileStream to save the .zip file
                    using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                    using (FileStream fileStream = File.Create(outputPath))
                    {
                        // Copy the content stream to the file stream
                        await contentStream.CopyToAsync(fileStream);
                    }

                    ParentWindow.PrintAsync("Download completed successfully.");
                }
                else
                {
                    ParentWindow.PrintAsync($"Failed to download. HTTP status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                ParentWindow.PrintAsync($"An error occurred: {ex.Message}");
            }
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
