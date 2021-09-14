namespace BitJuice.Backup.Modules.Aggregators
{
    public enum ArchiveBuilderState
    {
        OpenArchive,
        OpenFile,
        ReadFile,
        CloseFile,
        CloseArchive,
        Done,
    }
}