using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BitJuice.Backup.Infrastructure;
using BitJuice.Backup.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BitJuice.Backup.Modules.Providers
{
    [ModuleName("directory-provider")]
    public class DirectoryProvider : ModuleBase<DirectoryConfig>, IProvider
    {
        private readonly ILogger<DirectoryProvider> logger;
        private RewriteEngine rewriteEngine;

        public DirectoryProvider(ILogger<DirectoryProvider> logger)
        {
            this.logger = logger;
        }

        public override void Configure(IConfiguration config)
        {
            base.Configure(config);

            // Normalize excludes
            if (Config.Excludes != null && Config.Excludes.Any())
                Config.Excludes = Config.Excludes.Select(i => i.TrimEnd('/', '\\')).ToList();

            rewriteEngine = new RewriteEngine(Config.Rewrites ?? Enumerable.Empty<RewriteRule>());
        }

        public async IAsyncEnumerable<IDataItem> GetAsync()
        {
            await Task.CompletedTask;
            foreach (var path in Config.Paths)
            {
                logger.LogInformation($"Reading content of {path}");

                var directory = new DirectoryInfo(path);
                if (!directory.Exists)
                {
                    logger.LogWarning($"Directory does not exist {path}");
                    continue;
                }

                var pattern = string.IsNullOrWhiteSpace(Config.Pattern) ? "*" : Config.Pattern;
                var searchOption = Config.TopOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories;
                
                foreach (var dataItem in GetDataItems(directory, pattern, searchOption))
                    yield return dataItem;
            }
        }

        private IEnumerable<FileDataItem> GetDataItems(DirectoryInfo directory, string pattern, SearchOption searchOption)
        {
            foreach (var fileInfo in directory.EnumerateFiles(pattern, SearchOption.TopDirectoryOnly).Where(IsNotSymbolicLink))
                yield return NewDataItem(fileInfo);

            if (searchOption == SearchOption.TopDirectoryOnly)
                yield break;

            foreach (var directoryInfo in directory.EnumerateDirectories())
            {
                if (IsExcluded(directoryInfo))
                {
                    logger.LogInformation($"Skipping excluded directory: {directoryInfo.FullName}");
                    continue;
                }

                foreach (var fileDataItem in GetDataItems(directoryInfo, pattern, searchOption))
                    yield return fileDataItem;
            }
        }

        private bool IsExcluded(DirectoryInfo directoryInfo)
        {
            if (Config.Excludes == null)
                return false;
            return Config.Excludes.Any(exclude => directoryInfo.FullName.StartsWith(exclude, StringComparison.InvariantCultureIgnoreCase));
        }

        private static bool IsNotSymbolicLink(FileInfo fileInfo)
        {
            return !fileInfo.Attributes.HasFlag(FileAttributes.ReparsePoint);
        }

        private FileDataItem NewDataItem(FileInfo fileInfo)
        {
            return new FileDataItem(logger, fileInfo, rewriteEngine.Rewrite(fileInfo.FullName));
        }
    }
}