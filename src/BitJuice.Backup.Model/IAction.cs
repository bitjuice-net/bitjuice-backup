using System.Threading.Tasks;

namespace BitJuice.Backup.Model
{
    public interface IAction : IModule
    {
        Task ExecuteAsync();
    }
}
