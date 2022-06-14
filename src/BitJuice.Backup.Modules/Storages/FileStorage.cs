using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BitJuice.Backup.Infrastructure;
using BitJuice.Backup.Model;

namespace BitJuice.Backup.Modules.Storages
{
    [ModuleName("file-storage")]
    public class FileStorage : ModuleBase<FileConfig>, IStorage
    {
        public async Task PushAsync(IEnumerable<IDataItem> items)
        {
            foreach (var item in items)
            {
                await using var inputStream = item.GetStream();
                var path = Config.Path ?? string.Empty;
                path = Path.Combine(path, item.Name);
                await using var outputStream = File.Open(path, FileMode.Create);
                await inputStream.CopyToAsync(outputStream);
                outputStream.Close();
            }
        }
    }
}
