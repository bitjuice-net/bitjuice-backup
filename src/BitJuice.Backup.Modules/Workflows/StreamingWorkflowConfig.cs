using BitJuice.Backup.Model;

namespace BitJuice.Backup.Modules.Workflows
{
    public class StreamingWorkflowConfig : IModuleConfig
    {
        public string Description { get; set; }

        public void Validate()
        {
        }
    }
}
