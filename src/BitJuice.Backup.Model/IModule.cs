using Microsoft.Extensions.Configuration;

namespace BitJuice.Backup.Model
{
    public interface IModule
    {
        void Configure(IConfiguration config);
    }
}