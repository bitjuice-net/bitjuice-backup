using System.IO;

namespace BitJuice.Backup.Model
{
    public interface IDataItem
    {
        string Name { get; }
        string VirtualPath { get; }
        Stream GetStream();
    }
}