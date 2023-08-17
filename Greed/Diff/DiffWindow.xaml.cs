using Greed.Models;
using Greed.Models.JsonSource;
using Greed.Models.ListItem;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Greed
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class DiffWindow : Window
    {
        private Source Source;

        public DiffWindow(Source s)
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
