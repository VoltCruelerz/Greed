using Greed.Controls.Popups;
using System;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Common;
using System.Diagnostics;

namespace Greed.Updater
{
    public static class UpdateManager
    {
        public static async Task<bool> UpdateGreed(Version version)
        {
            try
            {
                var curDir = System.AppDomain.CurrentDomain.BaseDirectory;
                var url = $"https://github.com/VoltCruelerz/Greed/releases/download/{version}/Greed.{version}.zip";
                var zipFile = $"Greed.{version}.zip";
                var extractFolder = $"Greed.{version}";
                _ = MainWindow.Instance!.PrintAsync($"Installing {zipFile}...");

                // Download the zip
                var zipPath = Path.Combine(ConfigurationManager.AppSettings["downDir"]!, zipFile);
                var extractPath = Path.Combine(ConfigurationManager.AppSettings["downDir"]!, extractFolder);
                if (File.Exists(zipPath))
                {
                    File.Delete(zipPath);
                }
                if (!await DownloadZipFile(url, zipPath))
                {
                    return false;
                }
                _ = MainWindow.Instance!.PrintAsync($"Download of {zipFile} to {zipPath} complete.");

                // Extract the zip
                ExtractArchive(zipPath, extractPath);
                File.Delete(zipPath);

                // Purge own directory of non-essentials
                var ownContents = Directory.GetFiles(curDir);
                foreach ( var file in ownContents )
                {
                    if (!file.EndsWith("Greed.exe")) {
                        File.Delete(file);
                    }
                }
                _ = MainWindow.Instance!.PrintAsync($"Self purge of {curDir} complete.");

                // Copy the files
                var updatedContents = Directory.GetFiles(extractPath);
                foreach (var file in updatedContents)
                {

                    var filename = file.EndsWith("Greed.exe")
                        ? "Greed_New.exe"
                        : file.Split('\\')[^1];
                    var dest = Path.Combine(curDir, filename);
                    await MoveFileWithRetries(file, dest);
                }
                _ = MainWindow.Instance!.PrintAsync($"Copy to {curDir} complete.");

                _ = MainWindow.Instance!.PrintAsync($"Starting restarter...");

                Process.Start(Path.Combine(curDir, "Greed.AutoUpdater.exe"), Environment.ProcessId.ToString());
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                CriticalAlertPopup.Throw("Failed to update Greed", ex);
                return false;
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
                            File.Move(src, dest);
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
    }
}
