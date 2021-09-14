using System.IO;
using BitJuice.Backup.Model;
using Microsoft.Extensions.Logging;

namespace BitJuice.Backup.Modules.Providers
{
    public class FileDataItem : IDataItem
    {
        private readonly ILogger<DirectoryProvider> logger;
        private readonly FileInfo fileInfo;
        private readonly string virtualPath;

        public string Name => fileInfo.Name;
        public string VirtualPath => virtualPath ?? fileInfo.FullName;

        public FileDataItem(ILogger<DirectoryProvider> logger, FileInfo fileInfo)
        {
            this.logger = logger;
            this.fileInfo = fileInfo;
        }

        public FileDataItem(ILogger<DirectoryProvider> logger, FileInfo fileInfo, string virtualPath) : this(logger, fileInfo)
        {
            this.virtualPath = virtualPath;
        }

        public Stream GetStream()
        {
            logger.LogDebug($"Opening file: {fileInfo.FullName}, Length: {fileInfo.Length}");
            return new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }
    }
}