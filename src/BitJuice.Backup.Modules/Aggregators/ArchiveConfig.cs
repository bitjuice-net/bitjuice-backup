using BitJuice.Backup.Infrastructure;
using BitJuice.Backup.Model;

namespace BitJuice.Backup.Modules.Aggregators
{
    public class ArchiveConfig : IModuleConfig
    {
        public string Filename { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Filename))
                throw new InvalidConfigException<ArchiveConfig>("Filename is required");
        }
    }
}
