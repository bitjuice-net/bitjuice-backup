using BitJuice.Backup.Model;
using Microsoft.Extensions.DependencyInjection;

namespace BitJuice.Backup.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBackupCore(this IServiceCollection serviceCollection, Action<BackupCoreOptions> configure)
    {
        return serviceCollection
            .Configure(configure)
            .AddTransient<WorkflowExecutor>()
            .AddSingleton<IModuleFactory, ModuleFactory>()
            .AddSingleton<IModuleRepository, ModuleRepository>();
    }
}