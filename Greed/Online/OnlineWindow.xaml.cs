using Greed.Models;
using Greed.Models.Json;
using Greed.Models.ListItem;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;

namespace Greed
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class OnlineWindow : Window
    {
        public OnlineListing Listing { get; set; }
        public MainWindow Parent { get; set; }

        public OnlineWindow(OnlineListing listing, MainWindow parent)
        {
            InitializeComponent();
            Debug.WriteLine("OnlineWindow()");
            Listing = listing;
            Parent = parent;

            RefreshOnlineModListUI();
        }

        private void RefreshOnlineModListUI()
        {
            Parent.PrintAsync($"RefreshOnlineModListUI for {Listing.Mods.Count} mods.");
            ViewOnlineModList.Items.Clear();
            Listing.Mods
                .ForEach(m => ViewOnlineModList.Items.Add(new OnlineListItem(m)));
        }

        private void OnlineModList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (OnlineListItem)e.AddedItems[0]!;
            var meta = Listing.Mods.First(m => m.Name == item.Name);

            var doc = new FlowDocument(new Paragraph(new Run($"{meta.Name} v{meta.Version} (Sins {meta.SinsVersion})")
            {
                FontWeight = FontWeights.Bold
            }));
            doc.Blocks.Add(new Paragraph(new Run($"by {meta.Author}")
            {
                FontStyle = FontStyles.Italic
            }));
            doc.Blocks.Add(new Paragraph(new Run(meta.Url)
            {
                TextDecorations = TextDecorations.Underline
            }));
            doc.Blocks.Add(new Paragraph(new Run(meta.Description)));
            if (meta.Dependencies.Any())
            {
                var p = new Paragraph(new Run("Dependencies")
                {
                    FontWeight = FontWeights.Bold
                });
                meta.Dependencies.ForEach(c => p.Inlines.Add(new Run("\r\n- " + c)));
                doc.Blocks.Add(p);
            }
            if (meta.Conflicts.Any())
            {
                var p = new Paragraph(new Run("Conflicts")
                {
                    FontWeight = FontWeights.Bold
                });
                meta.Conflicts.ForEach(c => p.Inlines.Add(new Run("\r\n- " + c)));
                doc.Blocks.Add(p);
            }

            TxtOnlineInfo.Document = doc;
        }
    }
}
