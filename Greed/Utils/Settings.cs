using Greed.Controls;
using Greed.Controls.Popups;
using Greed.Extensions;
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
        public static readonly List<double> SliderValue = new() { 0.1, 0.2, 0.35, 0.5, 0.65, 0.8, 0.9, 1.0, 1.1, 1.2, 1.3, 1.50, 1.75, 2.0, 2.5, 3, 4, 5, 6, 8, 10 };
        public static readonly int SliderOne = SliderValue.FindIndex(p => p == 1.0);

        private static readonly string ConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "Config.json");
        private static readonly Config Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigPath))!;
        private static readonly List<Expander> ScalarExpanders = new();

        public static void AutomigrateConfig()
        {
            var wall = "=================================" + Environment.NewLine;
            // Pre 2.7 (dll.config)
            if (Config.Version.IsOlderThan(new Version("2.7.0")))
            {
                Log.Info("Migrating from dll.config...");
                try
                {
                    const string ModDirKey = "modDir";
                    const string SinsDirKey = "sinsDir";
                    const string ExportDirKey = "exportDir";
                    const string DownDirKey = "downDir";
                    const string ChannelKey = "channel";
                    Log.Info($"{wall}- Migrating Mods Dir...{Environment.NewLine}- - JSON: {Config.Dirs.Mods}{Environment.NewLine}- - XML: {ConfigurationManager.AppSettings[ModDirKey]!}{Environment.NewLine}- - Default: {DefaultModDir}");
                    Config.Dirs.Mods = string.IsNullOrEmpty(Config.Dirs.Mods)
                        ? ConfigurationManager.AppSettings[ModDirKey]!
                        : DefaultModDir;
                    Log.Info("- Migrated to " + Config.Dirs.Mods);

                    Log.Info($"{wall}- Migrating Export Dir...{Environment.NewLine}- - JSON: {Config.Dirs.Export}{Environment.NewLine}- - XML: {ConfigurationManager.AppSettings[ExportDirKey]!}{Environment.NewLine}- - Default: {DefaultModDir}");
                    Config.Dirs.Export = string.IsNullOrEmpty(Config.Dirs.Export)
                        ? ConfigurationManager.AppSettings[ExportDirKey]!
                        : DefaultModDir;
                    Log.Info("- Migrated to " + Config.Dirs.Export);

                    Log.Info($"{wall}- Migrating Sins Dir...{Environment.NewLine}- - JSON: {Config.Dirs.Sins}{Environment.NewLine}- - XML: {ConfigurationManager.AppSettings[SinsDirKey]!}{Environment.NewLine}- - Default: {DefaultSinDir}");
                    Config.Dirs.Sins = string.IsNullOrEmpty(Config.Dirs.Sins)
                        ? ConfigurationManager.AppSettings[SinsDirKey]!
                        : DefaultSinDir;
                    Log.Info("- Migrated to " + Config.Dirs.Sins);

                    Log.Info($"{wall}- Migrating Down Dir...{Environment.NewLine}- - JSON: {Config.Dirs.Download}{Environment.NewLine}- - XML: {ConfigurationManager.AppSettings[DownDirKey]!}{Environment.NewLine}- - Default: {DefaultDownDir}");
                    Config.Dirs.Download = string.IsNullOrEmpty(Config.Dirs.Download)
                        ? ConfigurationManager.AppSettings[DownDirKey]!
                        : DefaultDownDir;
                    Log.Info("- Migrated to " + Config.Dirs.Download);

                    Log.Info($"{wall}- Migrating Channel...{Environment.NewLine}- - JSON: {Config.Channel}{Environment.NewLine}- - XML: {ConfigurationManager.AppSettings[ChannelKey]!}{Environment.NewLine}- - Default: live");
                    Config.Channel = string.IsNullOrEmpty(Config.Channel)
                        ? ConfigurationManager.AppSettings[ChannelKey]!
                        : "live";
                    Log.Info("- Migrated to " + Config.Channel);
                }
                catch (Exception ex)
                {
                    CriticalAlertPopup.ThrowAsync("Unable to import existing config. Defaulting.", ex);
                }
            }

            // Mark that the migration is complete
            if (Config.Version.IsOlderThan(GetGreedVersion()))
            {
                Config.Version = GetGreedVersion();
                Save();
            }
        }

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
            if (!File.Exists(GetSinsExePath()))
            {
                CriticalAlertPopup.ThrowAsync("Unable to locate " + GetSinsExePath(), new FileNotFoundException());
                return new Version(0, 0, 0);
            }
            return new Version(FileVersionInfo.GetVersionInfo(GetSinsExePath()).FileVersion!);
        }

        public static List<GlobalScalar> GetScalars()
        {
            List<GlobalScalar> scalars = new();
            Config.Groups.ForEach(g => g.Scalars.ForEach(s => scalars.Add(s)));
            return scalars;
        }
        #endregion

        #region Set
        public static void SetModDir(string value)
        {
            Config.Dirs.Mods = value;
            Save();
        }

        public static void SetSinsDir(string value)
        {
            Config.Dirs.Sins = value;
            Save();
        }

        public static void SetExportDir(string value)
        {
            Config.Dirs.Export = value;
            Save();
        }

        public static void SetDownDir(string value)
        {
            Config.Dirs.Download = value;
            Save();
        }

        public static void SetChannel(string value)
        {
            Config.Channel = value;
            Save();
        }

        public static void SetSlider(string groupName, string scalarName, Label label, double tickIndex)
        {
            var mult = SliderValue[(int)tickIndex];
            label.Content = mult.ToString("P0");
            Config.Groups.Find(g => g.Name == groupName)!.Scalars.Find(s => s.Name == scalarName)!.Value = mult;
            Save();
        }

        public static void SetSlider(int groupIndex, int scalarIndex, Label label, double tickIndex)
        {
            var mult = SliderValue[(int)tickIndex];
            label.Content = mult.ToString("P0");
            Config.Groups[groupIndex].Scalars[scalarIndex].Value = mult;
            Save();
        }

        public static void ResetSliders(Grid parent)
        {
            parent.Children.Clear();
            ScalarExpanders.Clear();
            Config.Groups.ForEach(g => g.Scalars.ForEach(s => s.Value = 1));
            Save();
            PopulateScalarExpander(parent);
        }

        private static void Save()
        {
            File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(Config, Formatting.Indented));
        }
        #endregion

        #region Initialize Scalar Sliders
        public static int PopulateScalarExpander(Grid grid)
        {
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
                    subGrid.Children.Add(new SliderBox(scalar.Name, i, j, SliderValue.IndexOf(scalar.Value)));
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
