using Greed.Models;
using Greed.Models.Metadata;
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
            var doc = new FlowDocument(new Paragraph(new Run($"{meta.Name} v{meta.GetVersion()} (Sins {meta.GetSinsVersion()})")
            {
                FontWeight = FontWeights.Bold
            }));
            doc.Blocks.Add(new Paragraph(new Run($"by {meta.Author}")
            {
                FontStyle = FontStyles.Italic
            }));
            doc.Blocks.Add(new Paragraph(new Run(meta.Url)
            {
                TextDecorations = System.Windows.TextDecorations.Underline
            }));
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
    }
}
