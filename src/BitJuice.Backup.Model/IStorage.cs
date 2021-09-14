using System.Collections.Generic;

namespace BitJuice.Backup.Model
{
    public interface IStorage : IModule
    {
        void Push(IEnumerable<IDataItem> items);
    }
}