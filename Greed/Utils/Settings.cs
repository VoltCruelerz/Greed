using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;

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
        public static void SetConfigOptions(string key, string value)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings[key].Value = value;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
        #endregion

    }
}
