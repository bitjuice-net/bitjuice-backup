using System.IO;
using BitJuice.Backup.Model;

namespace BitJuice.Backup.Modules.Aggregators
{
    public class ArchiveDataItem : IDataItem
    {
        private readonly ArchiveBuilder builder;

        public string Name { get; }
        public string VirtualPath { get; }

        public ArchiveDataItem(string name, ArchiveBuilder builder)
        {
            this.builder = builder;
            Name = name;
            VirtualPath = string.Empty;
        }

        public Stream GetStream()
        {
            return new ArchiveStream(builder);
        }
    }
}
