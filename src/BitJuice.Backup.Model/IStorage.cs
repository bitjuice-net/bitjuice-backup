using System.Collections.Generic;
using System.Threading.Tasks;

namespace BitJuice.Backup.Model
{
    public interface IStorage : IModule
    {
        Task PushAsync(IAsyncEnumerable<IDataItem> items);
    }
}