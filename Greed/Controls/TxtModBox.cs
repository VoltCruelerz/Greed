using Greed.Extensions;
using Greed.Models;
using Greed.Models.Metadata;
using System;
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
                hyper.RequestNavigate += (sender, args) => meta.Url.NavigateToUrl();
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
                meta.GetDependencies().ForEach(c => p.Inlines.Add(new Run(Environment.NewLine + Constants.UNI_BULLET + " " + c)));
                doc.Blocks.Add(p);
            }

            // Predecessors
            if (meta.GetPredecessors().Any())
            {
                var p = new Paragraph(new Run("Load Order Predecessors")
                {
                    FontWeight = FontWeights.Bold
                });
                meta.GetPredecessors().ForEach(c => p.Inlines.Add(new Run(Environment.NewLine + Constants.UNI_BULLET + " " + c)));
                doc.Blocks.Add(p);
            }

            // Conflicts
            if (meta.GetConflicts().Any())
            {
                var p = new Paragraph(new Run("Conflicts")
                {
                    FontWeight = FontWeights.Bold
                });
                meta.GetConflicts().ForEach(c => p.Inlines.Add(new Run(Environment.NewLine + Constants.UNI_BULLET + " " + c)));
                doc.Blocks.Add(p);
            }

            localMod?.RenderReadme(doc.Blocks);

            Document = doc;
        }
    }
}
