using BitJuice.Backup.Model;

namespace BitJuice.Backup.Modules.Storages
{
    public class OneDriveConfig : IModuleConfig
    {
        public string ClientId { get; set; }
        public string TenantId { get; set; }

        public OneDriveConfig()
        {
        }

        public void Validate()
        {
        }
    }
}
