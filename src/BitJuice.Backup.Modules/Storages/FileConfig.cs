using BitJuice.Backup.Infrastructure;
using BitJuice.Backup.Model;

namespace BitJuice.Backup.Modules.Storages
{
    public class FileConfig : IModuleConfig
    {
        public string Path { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Path))
                throw new InvalidConfigException<FileConfig>("Path is required");
        }
    }
}