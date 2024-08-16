using System.Collections.Generic;

namespace BitJuice.Backup.Model
{
    public interface IAggregator : IModule
    {
        IAsyncEnumerable<IDataItem> Aggregate(IAsyncEnumerable<IDataItem> items);
    }
}