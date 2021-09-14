using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Reflection;
using BitJuice.Backup.Infrastructure;
using BitJuice.Backup.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace BitJuice.Backup
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var cronCommand = new Command("cron") ;
            cronCommand.AddCommand(new Command("install") { Handler = CommandHandler.Create(CronInstall) });
            cronCommand.AddCommand(new Command("uninstall") { Handler = CommandHandler.Create(CronUninstall) });

            var executeCommand = new Command("execute") { Handler = CommandHandler.Create<string>(Execute) };
            executeCommand.AddOption(new Option(new[] { "-c", "--config" }, "Add configuration file.") { Argument = new Argument<string>() });

            var rootCommand = new RootCommand("Backup Utility");
            rootCommand.AddCommand(cronCommand);
            rootCommand.AddCommand(executeCommand);
            rootCommand.Invoke(args);
        }

        private static void CronInstall()
        {
            var path = new FileInfo(Assembly.GetExecutingAssembly().Location);
            var dir = path.Directory?.FullName;

            var cron = new FileInfo("/etc/cron.daily/backup");
            using (var writer = new StreamWriter(cron.Create()))
            {
                writer.WriteLine("#!/bin/bash");
                writer.WriteLine();
                writer.WriteLine($"cd {dir}");
                writer.WriteLine("./Backup execute");
            }

            Process.StartProcess("chmod", $"+x {cron.FullName}");
        }

        private static void CronUninstall()
        {
            var cron = new FileInfo("/etc/cron.daily/backup");
            cron.Delete();
        }

        private static void Execute(string workflowFile)
        {
            CreateApp(workflowFile ?? "config/workflow.json").Run();
        }

        private static IApplicationService CreateApp(string workflowFile)
        {
            var appConfig = new ConfigurationBuilder().AddJsonFile("config/settings.json").Build();

            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton<IConfiguration>(appConfig);
            
            serviceCollection.AddLogging(builder =>
            {
                builder.AddSerilog(new LoggerConfiguration().ReadFrom.Configuration(appConfig).CreateLogger());
            });
            
            serviceCollection.Configure<ApplicationOptions>(i => i.WorkflowFile = workflowFile);

            serviceCollection.AddTransient<IApplicationService, ApplicationService>();
            serviceCollection.AddSingleton<IModuleFactory, ModuleFactory>();
            serviceCollection.AddSingleton<IModuleRepository, ModuleRepository>();

            var provider = serviceCollection.BuildServiceProvider();
            return provider.GetRequiredService<IApplicationService>();
        }
    }
}