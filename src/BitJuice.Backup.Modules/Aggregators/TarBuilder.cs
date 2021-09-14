using System.Collections.Generic;
using System.Text;
using BitJuice.Backup.Model;
using ICSharpCode.SharpZipLib.Tar;
using Microsoft.Extensions.Logging;

namespace BitJuice.Backup.Modules.Aggregators
{
    public class TarBuilder : ArchiveBuilder
    {
        private TarOutputStream tarStream;

        public TarBuilder(ILogger<ArchiveBuilder> logger, IEnumerable<IDataItem> items) : base(logger, items)
        {
        }

        protected override void OpenArchive()
        {
            tarStream = new TarOutputStream(OutputStream, Encoding.UTF8) { IsStreamOwner = false };
        }

        protected override void OpenFile()
        {
            var tarEntry = TarEntry.CreateTarEntry(CurrentItem.VirtualPath);
            tarEntry.Size = CurrentItemStream.Length;
            tarStream.PutNextEntry(tarEntry);
        }

        protected override void WriteFile(byte[] buffer, int count)
        {
            tarStream.Write(buffer, 0, count);
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
        }
    }
}