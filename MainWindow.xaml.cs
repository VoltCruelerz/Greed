using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Greed.Models;

namespace Greed
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Mod> Mods = new();

        public MainWindow()
        {
            Debug.WriteLine("Loading Settings...");
            string modDir = ConfigurationManager.AppSettings["modDir"]!;
            string sinsDir = ConfigurationManager.AppSettings["sinsDir"]!;
            Debug.WriteLine($"Mod Dir: {modDir}");
            Debug.WriteLine($"Sins II Dir: {sinsDir}");

            Debug.WriteLine("Loading mods...");
            Mods = ModLoader.LoadGreedyMods(modDir);

            Debug.WriteLine("Main Window");
            InitializeComponent();
            Debug.WriteLine("Load Done");

            RefreshModList();
        }

        private void RefreshModList()
        {
            viewModList.Items.Clear();
            Mods.ForEach(m => viewModList.Items.Add(new { Active = m.IsActive ? "✓" : " ", Name = m.Meta.Name, Version = m.Meta.Version, SinsVersion = m.Meta.SinsVersion }));
        }

        private void Activate_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Activate_Click()");
        }

        private void Deactivate_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Deactivate_Click()");
        }

        private void viewModList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
