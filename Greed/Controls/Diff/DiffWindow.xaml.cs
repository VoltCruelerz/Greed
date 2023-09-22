using Greed.Models.Json;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Greed.Controls.Diff
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class DiffWindow : Window
    {
        private readonly JsonSource Source;
        private readonly SolidColorBrush Addition = new(Colors.LightBlue);
        private readonly SolidColorBrush Removal = new(Colors.Pink);
        private readonly SolidColorBrush Mutation = new(Colors.LightYellow);
        private readonly SolidColorBrush Normal = new(Colors.White);

        public DiffWindow(JsonSource s)
        {
            Debug.WriteLine("DiffWindow()");
            Source = s;
            InitializeComponent();
            this.Title = Source.SourcePath;

            var diff = Source.DiffFromGold();

            txtGold.Document = new FlowDocument(new Paragraph(new Run(diff.Gold)));
            txtGreedy.Document = new FlowDocument(new Paragraph(new Run(diff.Greedy)));

            // Need to colorize
            var p = new Paragraph();
            var diffLines = diff.Diff.Split(Environment.NewLine);


            foreach (var line in diffLines)
            {
                var brush = Normal;
                var trimmed = line.Trim();
                var padStart = line.Length - trimmed.Length;

                if (trimmed.StartsWith("\"*"))
                {
                    brush = Mutation;
                    trimmed = "\"" + trimmed[2..];// Strip off the *
                }
                else if (trimmed.StartsWith("\"+"))
                {
                    brush = Addition;
                    trimmed = "\"" + trimmed[2..];// Strip off the +
                }
                else if (trimmed.StartsWith("\"-"))
                {
                    brush = Removal;
                    trimmed = "\"" + trimmed[2..];// Strip off the -
                }

                if (padStart > 0)
                {
                    p.Inlines.Add(new Run("".PadLeft(padStart)));
                }

                var r = new Run(trimmed.PadLeft(trimmed.Length) + Environment.NewLine)
                {
                    Background = brush
                };
                p.Inlines.Add(r);
            }
            txtDiff.Document = new FlowDocument(p);
        }
    }
}
