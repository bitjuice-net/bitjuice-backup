using System;
using System.Linq;
using System.Text.RegularExpressions;
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

            try
            {
                var cts = new CancellationTokenSource();

                var timeoutTask = Task.Delay(Config.TimeoutMs, cts.Token);

                var nameRegex = new Regex($"^/{string.Join(".*?", Config.ContainerName.Split('*').Select(Regex.Escape))}$");
                var containers = await client.Containers.ListContainersAsync(new ContainersListParameters { All = true }, cts.Token);
                var filteredContainers = containers.Where(i => i.Names.Any(j => nameRegex.IsMatch(j))).ToList();

                foreach (var container in filteredContainers)
                {
                    logger.LogInformation($"Executing docker command: docker {Config.Command} {container.Names.First()} [{container.ID}]");
                    var commandTask = ExecuteCommand(client, container.ID, cts.Token);
                    var completedTask = await Task.WhenAny(commandTask, timeoutTask);
                    if (completedTask == timeoutTask)
                    {
                        cts.Cancel();
                        throw new Exception("Docker command reached maximum execution time, aborting.");
                    }

                    await completedTask;
                }
            }
            catch (Exception exception)
            {
                if (!Config.ContinueOnError)
                    throw;

                logger.LogWarning(exception.Message);
            }
        }

        private Task<bool> ExecuteCommand(DockerClient client, string id, CancellationToken cancellationToken) => Config.Command switch
        {
            DockerActionCommand.Start => client.Containers.StartContainerAsync(id, new ContainerStartParameters(), cancellationToken),
            DockerActionCommand.Stop => client.Containers.StopContainerAsync(id, new ContainerStopParameters(), cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(Config.Command), $"Invalid command: {Config.Command}")
        };
    }
}
