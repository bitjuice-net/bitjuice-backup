using System.CommandLine;
using System.CommandLine.Invocation;

namespace BitJuice.Backup
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var cronCommand = new Command("cron") ;
            cronCommand.AddCommand(new Command("install") { Handler = CommandHandler.Create(CronCommands.Install) });
            cronCommand.AddCommand(new Command("uninstall") { Handler = CommandHandler.Create(CronCommands.Uninstall) });

            var executeCommand = new Command("execute") { Handler = CommandHandler.Create<string>(AppCommands.Execute) };
            executeCommand.AddOption(new Option(new[] { "-c", "--config" }, "Add configuration file.") { Argument = new Argument<string>() });

            var rootCommand = new RootCommand("Backup Utility");
            rootCommand.AddCommand(cronCommand);
            rootCommand.AddCommand(executeCommand);
            rootCommand.Invoke(args);
        }
    }
}