using BitJuice.Backup.Model;

namespace BitJuice.Backup.Modules.Storages
{
    public class GoogleDriveConfig : IModuleConfig
    {
        public string FolderId { get; set; }
        public GoogleCredentialsType CredentialsType { get; set; }
        public string CredentialsFile { get; set; }
        public string TokensDir { get; set; }

        public GoogleDriveConfig()
        {
            CredentialsType = GoogleCredentialsType.User;
            CredentialsFile = "config/credentials.json";
            TokensDir = "tokens";
        }

        public void Validate()
        {
        }
    }
}