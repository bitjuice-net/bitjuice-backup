namespace BitJuice.Backup.Core
{
    public class BackupCoreOptions
    {
        public string File { get; set; } = string.Empty;
        public List<string> Assemblies { get; set; } = new();
    }
}
