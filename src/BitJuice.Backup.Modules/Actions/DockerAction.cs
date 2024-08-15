using System;
using System.Threading;
using System.Threading.Tasks;
using BitJuice.Backup.Infrastructure;
using BitJuice.Backup.Model;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;

namespace BitJuice.Backup.Modules.Actions
{
    [ModuleName("docker-action")]
    public class DockerAction : ModuleBase<DockerActionConfig>, IAction
    {
        private readonly ILogger<DockerAction> logger;

        public DockerAction(ILogger<DockerAction> logger)
        {
            this.logger = logger;
        }

        public async Task ExecuteAsync()
        {
            var config = string.IsNullOrWhiteSpace(Config.Endpoint) 
                ? new DockerClientConfiguration() 
                : new DockerClientConfiguration(new Uri(Config.Endpoint));
            var client = config.CreateClient();

            logger.LogInformation($"Executing docker command: docker {Config.Command} {Config.ContainerName}");

            try
            {
                var cts = new CancellationTokenSource();

                var commandTask = ExecuteCommand(client, cts.Token);
                var timeoutTask = Task.Delay(Config.TimeoutMs, cts.Token);

                var completedTask = await Task.WhenAny(commandTask, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    cts.Cancel();
                    throw new Exception("Docker command reached maximum execution time, aborting.");
                }

                await completedTask;
            }
            catch (Exception exception)
            {
                if (!Config.ContinueOnError)
                    throw;

                logger.LogWarning(exception.Message);
            }
        }

        private Task<bool> ExecuteCommand(DockerClient client, CancellationToken cancellationToken)
        {
            if (string.Equals(Config.Command, "start", StringComparison.OrdinalIgnoreCase))
                return client.Containers.StartContainerAsync(Config.ContainerName, new ContainerStartParameters(), cancellationToken);
            if (string.Equals(Config.Command, "stop", StringComparison.OrdinalIgnoreCase))
                return client.Containers.StopContainerAsync(Config.ContainerName, new ContainerStopParameters(), cancellationToken);
            throw new ArgumentOutOfRangeException(nameof(Config.Command), $"Invalid command: {Config.Command}");
        }
    }
}
