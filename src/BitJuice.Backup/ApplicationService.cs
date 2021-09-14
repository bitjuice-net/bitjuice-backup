using System;
using BitJuice.Backup.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BitJuice.Backup
{
    public class ApplicationService : IApplicationService
    {
        private readonly ILogger<ApplicationService> logger;
        private readonly IOptions<ApplicationOptions> options;
        private readonly IModuleFactory moduleFactory;

        public ApplicationService(ILogger<ApplicationService> logger, IOptions<ApplicationOptions> options, IModuleFactory moduleFactory)
        {
            this.logger = logger;
            this.options = options;
            this.moduleFactory = moduleFactory;
        }

        public void Run()
        {
            try
            {
                var config = new ConfigurationBuilder().AddJsonFile(options.Value.WorkflowFile).Build();
                var workflow = moduleFactory.Create<IWorkflow>(config);
                workflow.Run();
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "An error occurred during workflow execution.");
            }
        }
    }
}