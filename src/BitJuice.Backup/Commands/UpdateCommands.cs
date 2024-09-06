using BitJuice.Backup.Model.Update;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BitJuice.Backup.Commands
{
    public class UpdateCommands
    {
        private const string updateDirName = "update";
        private const string updateZipName = "update.zip";
        private const string latestReleaseUrl = "https://api.github.com/repos/bitjuice-net/bitjuice-backup/releases/latest";

        public static async Task Update(int stage, int processId)
        {
            switch (stage)
            {
                case 0:
                    await UpdateStage1(stage, processId);
                    break;
                case 1:
                    await UpdateStage2(stage, processId);
                    break;
                case 2:
                    await UpdateStage3(stage, processId);
                    break;
            }
        }

        private static async Task UpdateStage1(int stage, int processId)
        {
            var currentVersion = GetCurrentVersion();
            var latestVersion = await GetLatestVersionAsync();
            if (latestVersion <= currentVersion)
            {
                Console.WriteLine("No new version available");
                return;
            }

            Console.WriteLine($"New version available: {latestVersion}");
            Console.Write("Do you want to update [y/N]: ");
            var line = Console.ReadLine();
            if (line?.ToLower() != "y")
            {
                return;
            }

            await DownloadUpdateAsync();
            StartStageProcess(stage + 1, updateDirName);
        }

        private static async Task UpdateStage2(int stage, int processId)
        {
            Console.WriteLine("Updating files");

            await WaitForStageProcess(processId);

            var currentDir = new DirectoryInfo(AppContext.BaseDirectory);
            var parentDir = currentDir.Parent;
            var oldFiles = parentDir.EnumerateFiles("BitJuice.Backup*");
            foreach (var fileInfo in oldFiles)
            {
                fileInfo.Delete();
            }
            var newFiles = currentDir.EnumerateFiles("BitJuice.Backup*");
            foreach (var fileInfo in newFiles)
            {
                fileInfo.CopyTo(Path.Combine(parentDir.FullName, fileInfo.Name));
            }

            StartStageProcess(stage + 1, "..");
        }

        private static async Task UpdateStage3(int stage, int processId)
        {
            Console.WriteLine("Cleaning up");

            await WaitForStageProcess(processId);

            var updateDir = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, updateDirName));
            if (updateDir.Exists)
                updateDir.Delete(true);
            
            Console.WriteLine("Update completed");
        }

        private static void StartStageProcess(int stage, string directory)
        {
            var currentProcess = Process.GetCurrentProcess();
            var workingDirectory = Path.Combine(AppContext.BaseDirectory, directory);
            Process.Start(new ProcessStartInfo
            {
                FileName = Path.Combine(workingDirectory, currentProcess.MainModule.ModuleName),
                WorkingDirectory = workingDirectory,
                ArgumentList =
                {
                    "update",
                    "--stage",
                    stage.ToString(),
                    "--processId",
                    currentProcess.Id.ToString()
                }
            });
        }

        private static async Task WaitForStageProcess(int processId)
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
            var release = await client.GetFromJsonAsync<GithubRelease>(latestReleaseUrl);
            return Version.Parse(release.TagName.Substring(1));
        }

        private static async Task DownloadUpdateAsync()
        {
            Console.WriteLine("Downloading update");

            var assetName = GetAssetName();
            
            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Bitjuice.Backup", "1.0.0"));
            var release = await client.GetFromJsonAsync<GithubRelease>(latestReleaseUrl);
            var asset = release.Assets.SingleOrDefault(i => string.Equals(i.Name, assetName, StringComparison.OrdinalIgnoreCase));

            if (asset is null)
            {
                Console.WriteLine("Cannot find asset: " + assetName);
                Console.WriteLine("Available assets:");
                foreach (var releaseAsset in release.Assets)
                {
                    Console.WriteLine("\t" + releaseAsset.Name);
                }
                return;
            }

            await using (var file = File.OpenWrite(updateZipName))
            {
                await using var stream = await client.GetStreamAsync(asset.BrowserDownloadUrl);
                await stream.CopyToAsync(file);
            }

            var updateDir = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, updateDirName));
            if (updateDir.Exists)
                updateDir.Delete(true);

            Console.WriteLine("Extracting update");
            ZipFile.ExtractToDirectory(updateZipName, updateDirName);

            var updateFile = new FileInfo(Path.Combine(AppContext.BaseDirectory, updateZipName));
            if (updateFile.Exists)
                updateFile.Delete();
        }

        private static string GetAssetName()
        {
            var sb = new StringBuilder();
            if (OperatingSystem.IsWindows())
                sb.Append("win");
            else if (OperatingSystem.IsLinux())
                sb.Append("linux");
            else if (OperatingSystem.IsMacOS())
                sb.Append("osx");
            else
                throw new NotSupportedException("Not supported operating system");
            
            if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
                sb.Append("-arm64");
            else if(RuntimeInformation.ProcessArchitecture == Architecture.X64)
                sb.Append("-x64");
            else
                throw new NotSupportedException("Not supported operating architecture");
                
            if (IsSelfContained())
                sb.Append("-sc");
            
            sb.Append(".zip");
            return sb.ToString();
        }

        private static bool IsSelfContained()
        {
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            return assembly.GetCustomAttribute<SelfContainedAttribute>()?.SelfContained ?? true;
        }
    }
}