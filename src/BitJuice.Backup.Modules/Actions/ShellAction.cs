using System;
using System.Diagnostics;
using BitJuice.Backup.Infrastructure;
using BitJuice.Backup.Model;
using Microsoft.Extensions.Logging;

namespace BitJuice.Backup.Modules.Actions
{
    [ModuleName("shell-action")]
    public class ShellAction : ModuleBase<ShellActionConfig>, IAction
    {
        private readonly ILogger<ShellAction> logger;

        public ShellAction(ILogger<ShellAction> logger)
        {
            this.logger = logger;
        }

        public void Execute()
        {
            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                FileName = Config.Command
            };

            if (Config.Arguments != null)
                logger.LogInformation($"Executing command: {Config.Command} {string.Join(' ', Config.Arguments)}");
            else
                logger.LogInformation($"Executing command: {Config.Command}");

            if (Config.Arguments != null)
                foreach (var argumentsList in Config.Arguments) 
                    startInfo.ArgumentList.Add(argumentsList);
            try
            {
                var process = Process.Start(startInfo);
                if (process == null)
                    throw new Exception("Cannot execute process: " + Config.Command);

                if (!process.WaitForExit(Config.TimeoutMs))
                {
                    process.Kill();
                    throw new Exception("Process reached maximum execution time, aborting.");
                }

                if (process.ExitCode != 0)
                    throw new Exception($"Process exited with status: {process.ExitCode}");
            }
            catch(Exception exception)
            {
                if (!Config.ContinueOnError) 
                    throw;

                logger.LogWarning(exception.Message);
            }
        }
    }
}
