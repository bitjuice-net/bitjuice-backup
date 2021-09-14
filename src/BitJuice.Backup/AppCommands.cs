using System;
using BitJuice.Backup.Infrastructure;
using BitJuice.Backup.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace BitJuice.Backup
{
    public class AppCommands
    {
        public static void Execute(string workflowFile)
        {
            var services = ConfigureServices(workflowFile ?? "config/workflow.json");
            var executor = services.GetRequiredService<WorkflowExecutor>();
            executor.Run();
        }

        private static IServiceProvider ConfigureServices(string workflowFile)
        {
            var appConfig = new ConfigurationBuilder().AddJsonFile("config/settings.json").Build();

            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton<IConfiguration>(appConfig);
            
            serviceCollection.AddLogging(builder =>
            {
                builder.AddSerilog(new LoggerConfiguration().ReadFrom.Configuration(appConfig).CreateLogger());
            });

            serviceCollection.Configure<WorkflowExecutorOptions>(i => i.WorkflowFile = workflowFile);

            serviceCollection.AddTransient<WorkflowExecutor>();
            serviceCollection.AddSingleton<IModuleFactory, ModuleFactory>();
            serviceCollection.AddSingleton<IModuleRepository, ModuleRepository>();

            return serviceCollection.BuildServiceProvider();
        }
    }
}