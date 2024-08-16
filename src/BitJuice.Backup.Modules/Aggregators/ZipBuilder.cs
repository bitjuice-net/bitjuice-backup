using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using BitJuice.Backup.Model;
using Microsoft.Extensions.Logging;

namespace BitJuice.Backup.Modules.Aggregators
{
    public class ZipBuilder : ArchiveBuilder
    {
        private static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        private ZipArchive zipArchive;
        private ZipArchiveEntry currentEntry;
        private Stream currentEntryStream;

        public ZipBuilder(ILogger<ArchiveBuilder> logger, IAsyncEnumerable<IDataItem> items) : base(logger, items)
        {
        }

        protected override void OpenArchive()
        {
            zipArchive = new ZipArchive(OutputStream, ZipArchiveMode.Create, true);
        }

        protected override void OpenFile()
        {
            var path = AdjustPath(CurrentItem.VirtualPath);
            currentEntry = zipArchive.CreateEntry(path);
            currentEntryStream = currentEntry.Open();
        }

        protected override void WriteFile(byte[] buffer, int count)
        {
            currentEntryStream.Write(buffer, 0, count);
        }

        protected override void CloseFile()
        { 
            currentEntryStream.Close();
            currentEntryStream = null;
        }

        protected override void CloseArchive()
        {
            zipArchive.Dispose();
            zipArchive = null;
        }

        private string AdjustPath(string path)
        {
            if (IsWindows && Path.IsPathRooted(path))
            {
                var root = Path.GetPathRoot(path);
                path = Path.GetRelativePath(root!, path);
                path = Path.Combine(root.ToLower().Replace(":\\", "_drive"), path);
            }
            return path;
        }
    }
}