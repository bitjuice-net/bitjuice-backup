using BitJuice.Backup.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BitJuice.Backup.Core
{
    public class WorkflowExecutor
    {
        private readonly ILogger<WorkflowExecutor> logger;
        private readonly IOptions<WorkflowExecutorOptions> options;
        private readonly IModuleFactory moduleFactory;

        public WorkflowExecutor(ILogger<WorkflowExecutor> logger, IOptions<WorkflowExecutorOptions> options, IModuleFactory moduleFactory)
        {
            this.logger = logger;
            this.options = options;
            this.moduleFactory = moduleFactory;
        }

        public async Task RunAsync()
        {
            try
            {
                var config = new ConfigurationBuilder().AddJsonFile(options.Value.WorkflowFile).Build();
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