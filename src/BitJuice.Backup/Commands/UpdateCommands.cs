using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace BitJuice.Backup.Commands
{
    public class UpdateCommands
    {
        private const string updateDirName = "update";
        private const string updateZipName = "update.zip";

        public static async Task Update(string action, int processId)
        {
            if (action == "replace")
            {
                Console.WriteLine("Updating files");

                await WaitForProcess(processId);

                var currentDir = new DirectoryInfo(AppContext.BaseDirectory);
                var parentDir = currentDir.Parent;
                var oldFiles = parentDir.EnumerateFiles("BitJuice.Backup*").ToList();
                foreach (var fileInfo in oldFiles)
                {
                    fileInfo.Delete();
                }
                var newFiles = currentDir.EnumerateFiles("BitJuice.Backup*");
                foreach (var fileInfo in newFiles)
                {
                    fileInfo.CopyTo(Path.Combine(parentDir.FullName, fileInfo.Name));
                }

                StartProcess("..", "cleanup");
            }
            else if (action == "cleanup")
            {
                Console.WriteLine("Cleaning up");

                await WaitForProcess(processId);

                var updateDir = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, updateDirName));
                updateDir.Delete(true);
                var updateFile = new FileInfo(Path.Combine(AppContext.BaseDirectory, updateZipName));
                updateFile.Delete();

                Console.WriteLine("Update completed");
            }
            else
            {
                var currentVersion = GetCurrentVersion();
                var latestVersion = await GetLatestVersionAsync();
                if (latestVersion > currentVersion)
                {
                    Console.WriteLine($"New version available: {latestVersion}");
                    await DownloadAsync();
                    StartProcess(updateDirName, "replace");
                }
            }
        }

        private static async Task WaitForProcess(int processId)
        {
            try
            {
                var process = Process.GetProcessById(processId);
                if (!process.HasExited)
                    await process.WaitForExitAsync();
            }
            catch 
            {
                // Ignore
            }
        }

        private static void StartProcess(string directory, string action)
        {
            var currentProcessId = Process.GetCurrentProcess().Id;
            var dir = Path.Combine(AppContext.BaseDirectory, directory);
            var startInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(dir, "Bitjuice.Backup.exe"),
                WorkingDirectory = dir,
                ArgumentList =
                {
                    "update",
                    "--action",
                    action,
                    "--processId",
                    currentProcessId.ToString()
                }
            };
            Process.Start(startInfo);
        }

        private static Version GetCurrentVersion()
        {
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            var versionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            var versionString = versionAttribute is not null
                ? versionAttribute.InformationalVersion.Split('+').FirstOrDefault() ?? string.Empty
                : assembly.GetName().Version?.ToString() ?? string.Empty;
            return Version.Parse(versionString);
        }

        private static async Task<Version> GetLatestVersionAsync()
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Bitjuice.Backup", "1.0.0"));
            var jsonString = await client.GetStringAsync("https://api.github.com/repos/bitjuice-net/bitjuice-backup/releases/latest");
            var jsonObject = JsonNode.Parse(jsonString);
            var versionString = jsonObject["tag_name"].GetValue<string>().Substring(1);
            return Version.Parse(versionString);
        }

        private static async Task DownloadAsync()
        {
            Console.WriteLine("Downloading update");

            var assetName = GetAssetName();
            
            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Bitjuice.Backup", "1.0.0"));
            var jsonString = await client.GetStringAsync("https://api.github.com/repos/bitjuice-net/bitjuice-backup/releases/latest");
            var jsonObject = JsonNode.Parse(jsonString);
            var assets = jsonObject["assets"].AsArray();
            var asset = assets.SingleOrDefault(i => string.Equals(i["name"].GetValue<string>(), assetName, StringComparison.OrdinalIgnoreCase));
            var downloadUrl = asset["browser_download_url"].GetValue<string>();
            await using var stream = await client.GetStreamAsync(downloadUrl);
            await using var file = File.OpenWrite(updateZipName);
            await stream.CopyToAsync(file);

            var updateDir = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, updateDirName));
            if (updateDir.Exists)
                updateDir.Delete(true);

            Console.WriteLine("Extracting update");
            ZipFile.ExtractToDirectory(updateZipName, updateDirName);
        }

        private static string GetAssetName()
        {
            if (OperatingSystem.IsWindows())
                return "win-x64.zip";
            if (OperatingSystem.IsLinux())
                return "linux-x64.zip";
            throw new NotSupportedException();
        }
    }
}
