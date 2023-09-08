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

            var doc = new FlowDocument(new Paragraph(new Run($"{meta.Name} v{meta.GetVersion()} (Sins {meta.GetSinsVersion()})")
            {
                FontWeight = FontWeights.Bold
            }));
            doc.Blocks.Add(new Paragraph(new Run($"by {meta.Author}")
            {
                FontStyle = FontStyles.Italic
            }));

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

            doc.Blocks.Add(new Paragraph(new Run(meta.Description)));
            if (meta.GetDependencies().Any())
            {
                var p = new Paragraph(new Run("Dependencies")
                {
                    FontWeight = FontWeights.Bold
                });
                meta.GetDependencies().ForEach(c => p.Inlines.Add(new Run("\r\n- " + c)));
                doc.Blocks.Add(p);
            }
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

        private void GoToUrl(string url)
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
    }
}
