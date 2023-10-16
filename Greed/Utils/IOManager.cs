﻿using Greed.Controls.Popups;
using Greed.Models;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Common;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace Greed.Utils
{
    public static class IOManager
    {
        public static async Task UpdateGreed(Version version)
        {
            try
            {
                var curDir = System.AppDomain.CurrentDomain.BaseDirectory;
                var url = $"https://github.com/VoltCruelerz/Greed/releases/download/{version}/Greed.{version}.zip";
                var zipFile = $"Greed.{version}.zip";
                var extractFolder = $"Greed.{version}";
                var zipPath = Path.Combine(Settings.GetDownDir(), zipFile);
                var extractPath = Path.Combine(Settings.GetDownDir(), extractFolder);
                _ = MainWindow.Instance!.PrintAsync($"Installing {zipFile}...");

                // Clean up previous execution
                if (File.Exists(zipPath)) File.Delete(zipPath);
                if (Directory.Exists(extractPath)) Directory.Delete(extractPath, true);

                // Download the zip
                if (!await DownloadZipFile(url, zipPath)) return;
                _ = MainWindow.Instance!.PrintAsync($"Download of {zipFile} to {zipPath} complete.");

                // Extract the zip
                ExtractArchive(zipPath, extractPath);
                File.Delete(zipPath);

                // Copy the new files
                var updatedContents = Directory.GetFiles(Path.Combine(extractPath, $"Greed {version}")).Where(f => !f.EndsWith(".config"));
                foreach (var file in updatedContents)
                {
                    var filename = Path.GetFileName(file) + ".tmp";
                    var dest = Path.Combine(curDir, filename);
                    await MoveFileWithRetries(file, dest);
                }
                _ = MainWindow.Instance!.PrintAsync($"Copy to {curDir} complete.");

                _ = MainWindow.Instance!.PrintAsync($"Starting restarter...");

                var response = MessageBox.Show("Greed needs to restart itself to finish the update. Would you like to restart it now?", "Greed Restart", MessageBoxButton.YesNo);
                if (response == MessageBoxResult.Yes)
                {
                    Process.Start(Path.Combine(curDir, "Greed.AutoUpdater.exe"), Environment.ProcessId.ToString());
                    Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                CriticalAlertPopup.ThrowAsync("Failed to update Greed", ex);
            }
        }

        public static async Task<bool> DownloadZipFile(string releaseUrl, string outputPath)
        {
            using HttpClient httpClient = new();
            try
            {
                // Send an HTTP GET request to the GitHub release URL
                await MainWindow.Instance!.PrintAsync("Downloading from " + releaseUrl);
                HttpResponseMessage response = await httpClient.GetAsync(releaseUrl);

                // Check if the request was successful (HTTP status code 200)
                if (response.IsSuccessStatusCode)
                {
                    // Get the response stream and create a FileStream to save the .zip file
                    using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                    using (FileStream fileStream = File.Create(outputPath))
                    {
                        // Copy the content stream to the file stream
                        await contentStream.CopyToAsync(fileStream);
                    }

                    await MainWindow.Instance!.PrintAsync("Download completed successfully.");
                }
                else
                {
                    throw new InvalidOperationException($"Failed to download. HTTP status code: {response.StatusCode}");
                }
                return true;
            }
            catch (Exception ex)
            {
                await MainWindow.Instance!.PrintAsync($"A download error occurred: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        public static void ExtractArchive(string archivePath, string extractPath)
        {
            var extension = Path.GetExtension(archivePath);
            if (extension == ".zip")
            {
                ZipFile.ExtractToDirectory(archivePath, extractPath);
            }
            else if (extension == ".rar")
            {
                Directory.CreateDirectory(extractPath);
                using var archive = RarArchive.Open(archivePath);
                foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                {
                    entry.WriteToDirectory(extractPath, new ExtractionOptions()
                    {
                        ExtractFullPath = true,
                        Overwrite = true
                    });
                }
            }
            else
            {
                throw new InvalidDataException("Unrecognized archive type: " + extension);
            }
        }

        /// <summary>
        /// Repeatedly attempts to move a directory that may or may not be locked down by ZipFile.ExtractToDirectory()
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <param name="maxRetries"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public static async Task MoveDirWithRetries(string src, string dest, int maxRetries = 10)
        {
            var needToSwitchDrives = src.Split(":")[0] != dest.Split(":")[0];
            for (var retry = 0; retry <= maxRetries; retry++)
            {
                if (Directory.Exists(src))
                {
                    try
                    {
                        if (needToSwitchDrives)
                        {
                            Extensions.Extensions.CopyDirectory(src, dest, true);
                        }
                        else
                        {
                            Directory.Move(src, dest);
                        }
                        return;
                    }
                    catch (Exception ex)
                    {
                        _ = MainWindow.Instance!.PrintAsync(ex, "[WARN]");
                        await Task.Delay(Math.Min(retry * 100, 1000)).ConfigureAwait(false);
                        _ = MainWindow.Instance!.PrintAsync($"Retrying move due to ZipFile not being thread safe (r={retry}/{maxRetries})");
                        if (retry == maxRetries)
                        {
                            throw;
                        }
                    }
                }
                else
                {
                    throw new DirectoryNotFoundException($"Internal extracted directory does not exist!");
                }
            }
        }

        /// <summary>
        /// Repeatedly attempts to move a file that may or may not be locked down by ZipFile.ExtractToDirectory()
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <param name="maxRetries"></param>
        /// <param name="forceCopy"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public static async Task MoveFileWithRetries(string src, string dest, int maxRetries = 10, bool forceCopy = false)
        {
            var needToSwitchDrives = src.Split(":")[0] != dest.Split(":")[0];
            for (var retry = 0; retry <= maxRetries; retry++)
            {
                if (File.Exists(src))
                {
                    try
                    {
                        if (needToSwitchDrives || forceCopy)
                        {
                            File.Copy(src, dest, true);
                        }
                        else
                        {
                            File.Move(src, dest, true);
                        }
                        return;
                    }
                    catch (Exception ex)
                    {
                        _ = MainWindow.Instance!.PrintAsync(ex, "[WARN]");
                        await Task.Delay(Math.Min(retry * 100, 1000)).ConfigureAwait(false);
                        _ = MainWindow.Instance!.PrintAsync($"Retrying move due to ZipFile not being thread safe (r={retry}/{maxRetries})");
                        if (retry == maxRetries)
                        {
                            throw;
                        }
                    }
                }
                else
                {
                    throw new FileNotFoundException($"Internal extracted file does not exist!");
                }
            }
        }

        public static string ZipMod(Mod m)
        {
            var zipName = m.Id + ".zip";
            var zipPath = m.GetDir() + ".zip";
            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }
            ZipFile.CreateFromDirectory(m.GetDir(), zipPath);
            return zipPath;
        }

        public static void ReadyDirForDelete(string dir)
        {
            SetAttributesNormal(new DirectoryInfo(dir));
        }

        public static void SetAttributesNormal(DirectoryInfo dir)
        {
            foreach (var subDir in dir.GetDirectories()) SetAttributesNormal(subDir);
            foreach (var file in dir.GetFiles())
            {
                file.Attributes = FileAttributes.Normal;
            }
        }
    }
}
