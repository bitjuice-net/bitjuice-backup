using BitJuice.Backup.Infrastructure;
using BitJuice.Backup.Model;

namespace BitJuice.Backup.Modules.Actions
{
    public class ShellActionConfig : IModuleConfig
    {
        public string Command { get; set; }
        public string[] Arguments { get; set; }
        public int TimeoutMs { get; set; } = 30 * 1000;
        public bool ContinueOnError { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Command))
                throw new InvalidConfigException<ShellActionConfig>("Command is required");
        }
    }
}