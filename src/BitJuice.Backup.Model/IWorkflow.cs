namespace BitJuice.Backup.Model
{
    public interface IWorkflow : IModule
    {
        void Run();
    }
}