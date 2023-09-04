using Greed.Models;
using Greed.Models.ListItem;
using Greed.Models.Metadata;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Greed.Controls.Online
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class OnlineWindow : Window
    {
        public OnlineListing Listing { get; set; }
        public MainWindow ParentWindow { get; set; }
        private OnlineMetadata? SelectedMod { get; set; }

        public OnlineWindow(OnlineListing listing, MainWindow parent)
        {
            InitializeComponent();
            Debug.WriteLine("OnlineWindow()");
            Listing = listing;
            ParentWindow = parent;

            RefreshOnlineModListUI();
        }

        private void RefreshOnlineModListUI()
        {
            ParentWindow.PrintAsync($"RefreshOnlineModListUI for {Listing.Mods.Count} mods.");
            ViewOnlineModList.Items.Clear();
            Listing.Mods
                .ForEach(m => ViewOnlineModList.Items.Add(new OnlineListItem(m)));
        }

        private void OnlineModList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (OnlineListItem)e.AddedItems[0]!;
            var meta = Listing.Mods.First(m => m.Name == item.Name);
            SelectedMod = meta;
            TxtOnlineInfo.SetContent(meta, null);
        }

        private void MenuInstall_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ParentWindow.PrintAsync($"Install({SelectedMod!.Id ?? SelectedMod?.Name})");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
