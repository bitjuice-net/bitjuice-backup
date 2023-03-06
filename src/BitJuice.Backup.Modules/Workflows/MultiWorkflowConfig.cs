using BitJuice.Backup.Model;

namespace BitJuice.Backup.Modules.Workflows;

public class MultiWorkflowConfig : IModuleConfig
{
    public string Description { get; set; }

    public void Validate()
    {
    }
}