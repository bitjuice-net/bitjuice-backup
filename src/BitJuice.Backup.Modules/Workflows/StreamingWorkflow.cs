using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BitJuice.Backup.Infrastructure;
using BitJuice.Backup.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BitJuice.Backup.Modules.Workflows
{
    [ModuleName("streaming-workflow")]
    public class StreamingWorkflow : ModuleBase<StreamingWorkflowConfig>, IWorkflow
    {
        private readonly ILogger logger;
        private readonly IModuleFactory factory;

        private IEnumerable<IAction> preActions;
        private IEnumerable<IAction> postActions;
        private IEnumerable<IProvider> providers;
        private IAggregator aggregator;
        private IStorage storage;

        public StreamingWorkflow(ILogger<StreamingWorkflow> logger, IModuleFactory factory)
        {
            this.logger = logger;
            this.factory = factory;
        }

        public override void Configure(IConfiguration config)
        {
            base.Configure(config);

            logger.LogInformation("Loading workflow modules");

            preActions = factory.CreateList<IAction>(config.GetSection("pre-actions"));
            postActions = factory.CreateList<IAction>(config.GetSection("post-actions"));
            providers = factory.CreateList<IProvider>(config.GetSection("providers"));

            if (providers == null || !providers.Any())
                throw new Exception("You need at least one provider");

            aggregator = factory.Create<IAggregator>(config.GetSection("aggregator"));
            if (aggregator == null)
                throw new Exception("You need to define an aggregator");

            storage = factory.Create<IStorage>(config.GetSection("storage"));
            if (storage == null)
                throw new Exception("You need to define a storage");
        }

        public async Task RunAsync()
        {
            if (string.IsNullOrWhiteSpace(Config.Description))
                logger.LogInformation("Starting workflow");
            else
                logger.LogInformation("Starting workflow: " + Config.Description);

            try
            {
                logger.LogInformation("Executing pre-actions");
                foreach (var action in preActions)
                    await action.ExecuteAsync();

                logger.LogInformation("Executing backup");
                var items1 = SelectMany(providers.Select(i => i.Get()));
                var items = aggregator.Aggregate(items1);
                await storage.PushAsync(items);
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "An error occurred during workflow execution.");
            }

            logger.LogInformation("Executing post-actions");
            foreach (var action in postActions)
                await action.ExecuteAsync();

            logger.LogInformation("Workflow finished");
        }

        private async IAsyncEnumerable<IDataItem> SelectMany(IEnumerable<IEnumerable<IDataItem>> lists)
        {
            await Task.CompletedTask;
            foreach (var list in lists)
            {
                foreach (var item in list)
                {
                    yield return item;
                }
            }
        }
    }
}