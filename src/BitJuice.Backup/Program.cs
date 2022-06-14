﻿using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace BitJuice.Backup
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var configOption = new Option<string>(new[] { "-c", "--config" }, "Add configuration file.");

            var cronInstallCommand = new Command("install");
            cronInstallCommand.SetHandler(CronCommands.Install);

            var cronUninstallCommand = new Command("uninstall");
            cronInstallCommand.SetHandler(CronCommands.Uninstall);

            var cronCommand = new Command("cron")
            {
                cronInstallCommand,
                cronUninstallCommand
            };

            var executeCommand = new Command("execute") {configOption};
            executeCommand.SetHandler(AppCommands.Execute, configOption);

            var rootCommand = new RootCommand("Backup Utility")
            {
                cronCommand,
                executeCommand
            };

            await rootCommand.InvokeAsync(args);
        }
    }
}