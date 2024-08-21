using System.CommandLine;
using System.Threading.Tasks;
using BitJuice.Backup.Commands;

namespace BitJuice.Backup
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var updateStageOption = new Option<int>(["-s", "--stage"]) { IsHidden = true };
            var updateProcessIdOption = new Option<int>(["-pid", "--processId"]) { IsHidden = true, };
            var updateCommand = new Command("update"){ updateStageOption, updateProcessIdOption };
            updateCommand.SetHandler(UpdateCommands.Update, updateStageOption, updateProcessIdOption);

            var cronInstallCommand = new Command("install");
            cronInstallCommand.SetHandler(CronCommands.Install);

            var cronUninstallCommand = new Command("uninstall");
            cronUninstallCommand.SetHandler(CronCommands.Uninstall);

            var cronCommand = new Command("cron")
            {
                cronInstallCommand,
                cronUninstallCommand
            };

            var configOption = new Option<string>(["-c", "--config"], "Add configuration file.");
            var executeCommand = new Command("execute") {configOption};
            executeCommand.SetHandler(AppCommands.Execute, configOption);

            var rootCommand = new RootCommand("Backup Utility")
            {
                updateCommand,
                cronCommand,
                executeCommand
            };

            await rootCommand.InvokeAsync(args);
        }
    }
}