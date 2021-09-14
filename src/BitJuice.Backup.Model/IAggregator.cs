using System.Collections.Generic;

namespace BitJuice.Backup.Model
{
    public interface IAggregator : IModule
    {
        IEnumerable<IDataItem> Aggregate(IEnumerable<IDataItem> items);
    }
}