using BitJuice.Backup.Model;
using Microsoft.Extensions.Configuration;

namespace BitJuice.Backup.Infrastructure
{
    public class ModuleBase : IModule
    {
        public virtual void Configure(IConfiguration config)
        {
        }
    }

    public class ModuleBase<TConfig> : IModule where TConfig : IModuleConfig
    {
        protected TConfig Config;

        public virtual void Configure(IConfiguration config)
        {
            Config = config.Get<TConfig>();
            Config.Validate();
        }
    }
}
