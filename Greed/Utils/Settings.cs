using Greed.Controls;
using Greed.Models.Config;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Greed.Utils
{
    public static class Settings
    {
        private static bool HasInitialized = false;

        public static readonly string DefaultModDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "Local", "sins2", "mods");
        public static readonly string DefaultSinDir = "C:\\Program Files\\Epic Games\\SinsII";
        public static readonly string DefaultDownDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

        // Sliders
        private static readonly List<double> SliderValue = new() { 0.1, 0.2, 0.35, 0.5, 0.65, 0.8, 0.9, 1.0, 1.1, 1.2, 1.3, 1.50, 1.75, 2.0, 2.5, 3, 4, 5, 6, 8, 10 };
        public static readonly int SliderOne = SliderValue.FindIndex(p => p == 1.0);

        private static readonly Config Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Config.json")))!;
        private static readonly List<Expander> ScalarExpanders = new();

        #region Get
        public static string GetModDir()
        {
            return Config.Dirs.Mods;
        }

        public static string GetSinsDir()
        {
            return Config.Dirs.Sins;
        }

        public static string GetExportDir()
        {
            return Config.Dirs.Export;
        }

        public static string GetDownDir()
        {
            return Config.Dirs.Download;
        }

        public static string GetChannel()
        {
            return Config.Channel;
        }

        public static double GetSliderTick(string groupName, string scalarName)
        {
            var result = Config.Groups.Find(g => g.Name == groupName)!.Scalars.Find(s => s.Name == scalarName)!.Value;
            return SliderValue.FindIndex(p => p == result);
        }

        public static double GetSliderMult(string groupName, string scalarName)
        {
            return Config.Groups.Find(g => g.Name == groupName)!.Scalars.Find(s => s.Name == scalarName)!.Value;
        }

        public static Version GetGreedVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version!;
        }

        public static string GetSinsExePath()
        {
            return GetSinsDir() + "\\sins2.exe";
        }

        public static Version GetSinsVersion()
        {
            return new Version(FileVersionInfo.GetVersionInfo(GetSinsExePath()).FileVersion!);
        }
        #endregion

        #region Set
        public static void SetModDir(string value)
        {
            Config.Dirs.Mods = value;
        }

        public static void SetSinsDir(string value)
        {
            Config.Dirs.Sins = value;
        }

        public static void SetExportDir(string value)
        {
            Config.Dirs.Export = value;
        }

        public static void SetDownDir(string value)
        {
            Config.Dirs.Download = value;
        }

        public static void SetChannel(string value)
        {
            Config.Channel = value;
        }

        public static void SetSlider(string groupName, string scalarName, Label label, double value)
        {
            var mult = SliderValue[(int)value];
            label.Content = (int)(mult * 100) + "%";
            Config.Groups.Find(g => g.Name == groupName)!.Scalars.Find(s => s.Name == scalarName)!.Value = value;
        }

        public static void SetSlider(int groupIndex, int scalarIndex, Label label, double value)
        {
            var mult = SliderValue[(int)value];
            label.Content = (int)(mult * 100) + "%";
            Config.Groups[groupIndex].Scalars[scalarIndex].Value = value;
        }
        #endregion

        #region Initialize Scalar Sliders
        public static int PopulateScalarExpander(Grid grid)
        {
            ScalarExpanders.Clear();
            var offset = 0;
            var white = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            for (var i = 0; i < Config.Groups.Count; i++)
            {
                var group = Config.Groups[i];
                var prevSet = offset;
                Grid subGrid = new()
                {
                    Background = white
                };
                for (var j = 0; j < group.Scalars.Count; j++)
                {
                    var scalar = group.Scalars[j];
                    subGrid.Children.Add(new SliderBox(scalar.Name, i, j));
                    offset++;
                }
                var subExp = new Expander()
                {
                    VerticalAlignment = System.Windows.VerticalAlignment.Top,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    Header = group.Name,
                    Height = 50 * group.Scalars.Count + 30,
                    Width = 537,
                    Content = subGrid,
                    Margin = new System.Windows.Thickness(0, i * 30, 0, 0)
                };

                subExp.Expanded += SubExp_Expanded;
                subExp.Collapsed += SubExp_Collapsed;

                ScalarExpanders.Add(subExp);
                grid.Children.Add(subExp);
                offset++;
            }
            return offset;
        }

        private static Thickness ShiftElementDown(Thickness original, double offset)
        {
            return new Thickness(original.Left, original.Top + offset, original.Right, original.Bottom);
        }

        private static Thickness ShiftElementUp(Thickness original, double offset)
        {
            return new Thickness(original.Left, original.Top - offset, original.Right, original.Bottom);
        }

        private static void SubExp_Expanded(object sender, RoutedEventArgs e)
        {
            var exp = (Expander)sender;
            var start = ScalarExpanders.IndexOf(exp) + 1;
            for (var i = start; i < ScalarExpanders.Count; i++)
            {
                var movable = ScalarExpanders[i];
                movable.Margin = ShiftElementDown(movable.Margin, exp.Height - 20);
            }
        }

        private static void SubExp_Collapsed(object sender, RoutedEventArgs e)
        {
            var exp = (Expander)sender;
            var start = ScalarExpanders.IndexOf(exp) + 1;
            for (var i = start; i < ScalarExpanders.Count; i++)
            {
                var movable = ScalarExpanders[i];
                movable.Margin = ShiftElementUp(movable.Margin, exp.Height - 20);
            }
        }
        #endregion

        public static void ExecGlobalScalars()
        {
            if (!HasInitialized)
            {
                Config.Init();
                HasInitialized = true;
            }

            Config.Groups.ForEach(g => g.Scalars.ForEach(s => s.Exec()));
        }
    }
}
