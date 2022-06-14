using System.Threading.Tasks;

namespace BitJuice.Backup.Model
{
    public interface IWorkflow : IModule
    {
        Task RunAsync();
    }
}