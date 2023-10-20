using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;

namespace Greed.Utils
{
    public static class Settings
    {
        public const string ModDirKey = "modDir";
        public const string SinsDirKey = "sinsDir";
        public const string ExportDirKey = "exportDir";
        public const string DownDirKey = "downDir";
        public const string ChannelKey = "channel";

        public static readonly string DefaultModDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "Local", "sins2", "mods");
        public static readonly string DefaultSinDir = "C:\\Program Files\\Epic Games\\SinsII";
        public static readonly string DefaultDownDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

        // Sliders
        public const string Supply = "fleetSupply";
        public const string NumTitans = "numTitans";
        public const string TacticalSlots = "tactical";
        public const string LogisticalSlots = "logistics";
        public const string CultureRate = "cultureRate";
        public const string BombingDamage = "bombingDamage";
        public const string WeaponDamage = "weaponDamage";
        public const string Antimatter = "antimatter";
        public const string UnitCost = "unitCost";
        public const string GravityWellSize = "gravityWellSize";
        public const string ExperienceForLevel = "experienceForLevel";
        private const string GlobalPrefix = "global.";
        private static readonly List<double> SliderValue = new() { 0.1, 0.2, 0.35, 0.5, 0.65, 0.8, 0.9, 1.0, 1.1, 1.2, 1.3, 1.50, 1.75, 2.0, 2.5, 3, 4, 5, 6, 8, 10 };
        public static readonly int SliderOne = SliderValue.FindIndex(p => p == 1.0);

        #region Get
        public static string GetModDir()
        {
            return ConfigurationManager.AppSettings[ModDirKey]!;
        }

        public static string GetSinsDir()
        {
            return ConfigurationManager.AppSettings[SinsDirKey]!;
        }

        public static string GetExportDir()
        {
            return ConfigurationManager.AppSettings[ExportDirKey]!;
        }

        public static string GetDownDir()
        {
            return ConfigurationManager.AppSettings[DownDirKey]!;
        }

        public static string GetChannel()
        {
            return ConfigurationManager.AppSettings[ChannelKey]!;
        }

        public static double GetSliderTick(string field)
        {
            var key = GlobalPrefix + field;
            var success = double.TryParse(ConfigurationManager.AppSettings[key]!, out double result);
            if (success) return SliderValue.FindIndex(p => p == result);
            return SliderOne;
        }

        public static double GetSliderMult(string field)
        {
            var key = GlobalPrefix + field;
            var success = double.TryParse(ConfigurationManager.AppSettings[key]!, out double result);
            return success ? result : 1;
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
            SetConfigOptions(ModDirKey, value);
        }

        public static void SetSinsDir(string value)
        {
            SetConfigOptions(SinsDirKey, value);
        }

        public static void SetExportDir(string value)
        {
            SetConfigOptions(ExportDirKey, value);
        }

        public static void SetDownDir(string value)
        {
            SetConfigOptions(DownDirKey, value);
        }

        public static void SetChannel(string value)
        {
            SetConfigOptions(ChannelKey, value);
        }

        public static void SetSlider(string field, Label label, double value)
        {
            var mult = SliderValue[(int)value];
            label.Content = (int)(mult * 100) + "%";
            SetConfigOptions(GlobalPrefix + field, mult + "");
        }

        public static void SetConfigOptions(string key, string value)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (config.AppSettings.Settings.AllKeys.Contains(key))
            {
                config.AppSettings.Settings[key].Value = value;
            }
            else
            {
                config.AppSettings.Settings.Add(key, value);
            }
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
        #endregion

    }
}
