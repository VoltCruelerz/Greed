﻿using Greed.Models;
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
    public partial class MainWindow : Window
    {
        private List<Mod> Mods = new();
        private Mod? Selected;

        public MainWindow()
        {
            Debug.WriteLine("Main Window");
            InitializeComponent();
            txtInfo.Document.Blocks.Clear();
            txtInfo.AppendText("Select a mod to view details about it.");
            Debug.WriteLine("Load Done");

            string? modDir = ConfigurationManager.AppSettings["modDir"];
            if (modDir == null || !Directory.Exists(modDir))
            {
                CriticalAlertPopup("Configuration Error", "You need to set the modDir element in Greed.dll.config file to your mod directory.");
                return;
            }
            string? sinsDir = ConfigurationManager.AppSettings["sinsDir"];
            if (sinsDir == null || !Directory.Exists(sinsDir))
            {
                CriticalAlertPopup("Configuration Error", "You need to set the sinsDir element in Greed.dll.config file to where sins2.exe lives.");
                return;
            }

            RefreshModList();
        }

        private void RefreshModList()
        {
            Debug.WriteLine("Loading Settings...");
            string modDir = ConfigurationManager.AppSettings["modDir"]!;
            string sinsDir = ConfigurationManager.AppSettings["sinsDir"]!;
            Debug.WriteLine($"Mod Dir: {modDir}");
            Debug.WriteLine($"Sins II Dir: {sinsDir}");
            try
            {
                this.Title = $"Greed Mod Loader (Detected Sins II v{FileVersionInfo.GetVersionInfo(sinsDir + "\\sins2.exe").FileVersion})";
            }
            catch (Exception)
            {
                CriticalAlertPopup("No SinsII", "sins2.exe could not be found at the specified location. Please double check that it is within the place indicated by the App.config.");
                return;
            }

            Debug.WriteLine("Loading mods...");
            try
            {
                Mods = ModManager.LoadGreedyMods();
            }
            catch (Exception e)
            {
                CriticalAlertPopup("Mod Load Error", "Unable to locate all files.\n" + e.Message + "\n" + e.StackTrace);
                return;
            }

            viewModList.Items.Clear();
            Mods.ForEach(m => viewModList.Items.Add(new ModListItem(m)));
        }

        private void ModList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // This gets hit when refreshing the display.
            if (e.AddedItems.Count == 0)
            {
                cmdToggle.IsEnabled = false;
                cmdToggle.Content = "Toggle";
                return;
            }
            var item = (ModListItem)e.AddedItems[0]!;
            Selected = Mods.First(p => p.Id == item.Id);

            var doc = new FlowDocument(new Paragraph(new Run($"{Selected.Meta.Name} v{Selected.Meta.Version} (Sins {Selected.Meta.SinsVersion})")
            {
                FontWeight = FontWeights.Bold
            }));
            doc.Blocks.Add(new Paragraph(new Run($"by {Selected.Meta.Author}")
            {
                FontStyle = FontStyles.Italic
            }));
            doc.Blocks.Add(new Paragraph(new Run(Selected.Meta.Url)
            {
                TextDecorations = TextDecorations.Underline
            }));
            doc.Blocks.Add(new Paragraph(new Run(Selected.Meta.Description)));
            txtInfo.Document = doc;

            // It starts disabled since nothing is selected.
            cmdToggle.IsEnabled = true;
            cmdToggle.Content = Selected.IsActive ? "Deactivate" : "Activate";
        }

        private void Toggle_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Activate_Click()");
            Selected!.IsActive = !Selected.IsActive;
            //Debug.WriteLine(Selected.DiffFromGold());
            string modDir = ConfigurationManager.AppSettings["modDir"]!;
            try
            {
                ModManager.SetGreedyMods(Mods.Where(m => m.IsActive).ToList());
            }
            catch (Exception ex)
            {
                CriticalAlertPopup("Mod Set Error", "Unable to locate all files.\n" + ex.Message + "\n" + ex.StackTrace);
                return;
            }
            RefreshModList();
            ReselectSelection();
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            cmdExport.IsEnabled = false;
            RefreshModList();
            ReselectSelection();

            try
            {
                ModManager.ExportGreedyMods(Mods.Where(m => m.IsGreedy && m.IsActive).ToList());
                MessageBox.Show("Greedy mods are now active. Have fun Sinning!", "Export Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                CriticalAlertPopup("Mod Export Error", "Unable to locate all files.\n" + ex.Message + "\n" + ex.StackTrace);
                return;
            }

            cmdExport.IsEnabled = true;
        }

        private void ReselectSelection()
        {
            Selected = Mods.Find(m => m.Id == Selected?.Id);
            if (Selected != null)
            {
                var index = Mods.IndexOf(Selected!);
                viewModList.SelectedItem = Selected;
                viewModList.SelectedIndex = index;
            }
        }

        private void CriticalAlertPopup(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            this.Close();
        }
    }
}
