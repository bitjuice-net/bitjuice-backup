using System;
using System.Threading.Tasks;
using BitJuice.Backup.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace BitJuice.Backup.Commands
{
    public class AppCommands
    {
        private const string DefaultSettingsFile = "config/settings.json";

        public static async Task Execute(string settingsFile)
        {
            var services = ConfigureServices(settingsFile);
            var executor = services.GetRequiredService<WorkflowExecutor>();
            await executor.RunAsync();
        }

        private static IServiceProvider ConfigureServices(string settingsFile)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile(settingsFile ?? DefaultSettingsFile)
                .Build();

            var logger = new LoggerConfiguration()
                .ReadFrom
                .Configuration(config)
                .CreateLogger();

            var serviceCollection = new ServiceCollection();

            serviceCollection.AddLogging(builder => builder.AddSerilog(logger));
            serviceCollection.AddBackupCore(i => config.GetSection("workflow").Bind(i));

            return serviceCollection.BuildServiceProvider();
        }
    }
}