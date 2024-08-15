using BitJuice.Backup.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BitJuice.Backup.Core
{
    public class WorkflowExecutor
    {
        private readonly ILogger<WorkflowExecutor> logger;
        private readonly IOptions<BackupCoreOptions> options;
        private readonly IModuleFactory moduleFactory;

        public WorkflowExecutor(ILogger<WorkflowExecutor> logger, IOptions<BackupCoreOptions> options, IModuleFactory moduleFactory)
        {
            this.logger = logger;
            this.options = options;
            this.moduleFactory = moduleFactory;
        }

        public async Task RunAsync()
        {
            try
            {
                var ext = Path.GetExtension(options.Value.File);
                var builder = new ConfigurationBuilder();
                if (string.Equals(ext, ".json", StringComparison.OrdinalIgnoreCase))
                    builder.AddJsonFile(options.Value.File);
                if (string.Equals(ext, ".yml", StringComparison.OrdinalIgnoreCase) || string.Equals(ext, ".yaml", StringComparison.OrdinalIgnoreCase))
                    builder.AddYamlFile(options.Value.File);
                var config = builder.Build();

                var workflow = moduleFactory.Create<IWorkflow>(config);
                await workflow.RunAsync();
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "An error occurred during workflow execution.");
            }
        }
    }
}