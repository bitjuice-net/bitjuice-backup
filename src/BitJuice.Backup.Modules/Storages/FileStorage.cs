using System.Collections.Generic;
using System.IO;
using BitJuice.Backup.Infrastructure;
using BitJuice.Backup.Model;

namespace BitJuice.Backup.Modules.Storages
{
    [ModuleName("file-storage")]
    public class FileStorage : ModuleBase<FileConfig>, IStorage
    {
        public void Push(IEnumerable<IDataItem> items)
        {
            foreach (var item in items)
            {
                using var inputStream = item.GetStream();
                var path = Config.Path ?? string.Empty;
                path = Path.Combine(path, item.Name);
                using var outputStream = File.Open(path, FileMode.Create);
                inputStream.CopyTo(outputStream);
                outputStream.Close();
            }
        }
    }
}
