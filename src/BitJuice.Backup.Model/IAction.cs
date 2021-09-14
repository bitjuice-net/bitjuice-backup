namespace BitJuice.Backup.Model
{
    public interface IAction : IModule
    {
        void Execute();
    }
}
