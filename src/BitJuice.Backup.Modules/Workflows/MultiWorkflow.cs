using System.Collections.Generic;
using System.Threading.Tasks;
using BitJuice.Backup.Infrastructure;
using BitJuice.Backup.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BitJuice.Backup.Modules.Workflows;

[ModuleName("multi-workflow")]
public class MultiWorkflow : ModuleBase<MultiWorkflowConfig>, IWorkflow
{
    private readonly ILogger<StreamingWorkflow> logger;
    private readonly IModuleFactory factory;
    private IEnumerable<IWorkflow> workflows;

    public MultiWorkflow(ILogger<StreamingWorkflow> logger, IModuleFactory factory)
    {
        this.logger = logger;
        this.factory = factory;
    }

    public override void Configure(IConfiguration config)
    {
        base.Configure(config);

        logger.LogInformation("Loading workflows");

        workflows = factory.CreateList<IWorkflow>(config.GetSection("workflows"));
    }

    public async Task RunAsync()
    {
        logger.LogInformation("Executing workflows");
        foreach (var workflow in workflows)
            await workflow.RunAsync();
    }
}