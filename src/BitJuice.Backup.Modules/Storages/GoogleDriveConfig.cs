using BitJuice.Backup.Model;

namespace BitJuice.Backup.Modules.Storages
{
    public class GoogleDriveConfig : IModuleConfig
    {
        public string FolderId { get; set; }
        public string CredentialsFile { get; set; }
        public string TokensDir { get; set; }

        public GoogleDriveConfig()
        {
            CredentialsFile = "config/credentials.json";
            TokensDir = "tokens";
        }

        public void Validate()
        {
        }
    }
}