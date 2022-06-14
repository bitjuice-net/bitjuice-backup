using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using BitJuice.Backup.Model;
using ICSharpCode.SharpZipLib.Tar;
using Microsoft.Extensions.Logging;

namespace BitJuice.Backup.Modules.Aggregators
{
    public class TgzBuilder : ArchiveBuilder
    {
        private GZipStream gzipStream;
        private TarOutputStream tarStream;

        public TgzBuilder(ILogger<ArchiveBuilder> logger, IEnumerable<IDataItem> items) : base(logger, items)
        {
        }

        protected override void OpenArchive()
        {
            gzipStream = new GZipStream(OutputStream, CompressionMode.Compress, true);
            tarStream = new TarOutputStream(gzipStream, Encoding.UTF8) { IsStreamOwner = false };
        }

        protected override void OpenFile()
        {
            var tarEntry = TarEntry.CreateTarEntry(CurrentItem.VirtualPath.Replace('\\', '/'));
            tarEntry.Size = CurrentItemStream.Length;
            tarStream.PutNextEntry(tarEntry);
        }

        protected override void WriteFile(byte[] buffer, int count)
        {
            tarStream.Write(buffer, 0, count);
            tarStream.Flush();
            gzipStream.Flush();
        }

        protected override void CloseFile()
        {
            tarStream.CloseEntry();
        }

        protected override void CloseArchive()
        {
            tarStream.Finish();
            tarStream.Close();
            tarStream = null;

            gzipStream.Close();
            gzipStream = null;
        }
    }
}