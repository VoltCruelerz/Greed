using Greed.Models.Json;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;

namespace Greed
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class DiffWindow : Window
    {
        private JsonSource Source;

        public DiffWindow(JsonSource s)
        {
            Debug.WriteLine("DiffWindow()");
            Source = s;
            InitializeComponent();
            this.Title = Source.SourcePath;

            var diff = Source.DiffFromGold();

            txtGold.Document = new FlowDocument(new Paragraph(new Run(diff.Gold)));
            txtDiff.Document = new FlowDocument(new Paragraph(new Run(diff.Diff)));
            txtGreedy.Document = new FlowDocument(new Paragraph(new Run(diff.Greedy)));
        }
    }
}
