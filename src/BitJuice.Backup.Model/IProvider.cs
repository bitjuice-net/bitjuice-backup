using System.Collections.Generic;

namespace BitJuice.Backup.Model
{
    public interface IProvider : IModule
    {
        IAsyncEnumerable<IDataItem> GetAsync();
    }
}