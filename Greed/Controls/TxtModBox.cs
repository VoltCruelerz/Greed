using Greed.Models;
using Greed.Models.Metadata;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Greed.Controls
{
    public class TxtModBox : RichTextBox
    {
        public void SetContent(BasicMetadata meta, Mod? localMod)
        {
            IsDocumentEnabled = true;

            // Basic
            var doc = new FlowDocument(new Paragraph(new Run($"{meta.Name} v{meta.GetVersion()} (Sins {meta.GetSinsVersion()})")
            {
                FontWeight = FontWeights.Bold
            }));

            // Total Conversion
            if (meta.IsTotalConversion)
            {
                doc.Blocks.Add(new Paragraph(new Run("Total Conversion")
                {
                    FontStyle = FontStyles.Italic,
                    FontWeight = FontWeights.Bold
                }));
            }

            // Author
            if (!string.IsNullOrEmpty(meta.Author))
            {
                doc.Blocks.Add(new Paragraph(new Run($"by {meta.Author}")
                {
                    FontStyle = FontStyles.Italic
                }));
            }

            // Url
            if (!string.IsNullOrEmpty(meta.Url))
            {
                var hyperParagraph = new Paragraph();
                var hyper = new Hyperlink
                {
                    IsEnabled = true,
                    NavigateUri = new Uri(meta.Url)
                };
                hyper.Inlines.Add(meta.Url);
                hyper.RequestNavigate += (sender, args) => GoToUrl(meta.Url);
                hyperParagraph.Inlines.Add(hyper);
                doc.Blocks.Add(hyperParagraph);
            }

            // Description
            if (!string.IsNullOrEmpty(meta.Description))
            {
                doc.Blocks.Add(new Paragraph(new Run(meta.Description)));
            }

            // Dependencies
            if (meta.GetDependencies().Any())
            {
                var p = new Paragraph(new Run("Dependencies")
                {
                    FontWeight = FontWeights.Bold
                });
                meta.GetDependencies().ForEach(c => p.Inlines.Add(new Run("\r\n- " + c)));
                doc.Blocks.Add(p);
            }

            // Conflicts
            if (meta.GetConflicts().Any())
            {
                var p = new Paragraph(new Run("Conflicts")
                {
                    FontWeight = FontWeights.Bold
                });
                meta.GetConflicts().ForEach(c => p.Inlines.Add(new Run("\r\n- " + c)));
                doc.Blocks.Add(p);
            }

            localMod?.RenderReadme(doc.Blocks);

            Document = doc;
        }

        private static void GoToUrl(string url)
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
    }
}
