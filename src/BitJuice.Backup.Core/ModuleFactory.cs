using BitJuice.Backup.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BitJuice.Backup.Core
{
    public class ModuleFactory : IModuleFactory
    {
        private const string DefaultModuleNameKey = "@module";
        private const string LegacyModuleNameKey = "module";

        private readonly IServiceProvider serviceProvider;
        private readonly IModuleRepository moduleRepository;

        public ModuleFactory(IServiceProvider serviceProvider, IModuleRepository moduleRepository)
        {
            this.serviceProvider = serviceProvider;
            this.moduleRepository = moduleRepository;
        }

        public T Create<T>(IConfiguration config) where T : IModule
        {
            var moduleName = config.GetSection(DefaultModuleNameKey)?.Value ?? config.GetSection(LegacyModuleNameKey)?.Value;
            if (moduleName == null)
                return default;

            var moduleType = moduleRepository.GetModuleInfo<T>(moduleName);
            if (!typeof(T).IsAssignableFrom(moduleType))
                throw new Exception($"Module '{moduleName}' of type '{moduleType?.FullName}' is not assignable from '{typeof(T).FullName}'");

            var module = (T)ActivatorUtilities.CreateInstance(serviceProvider, moduleType);
            module.Configure(config);
            return module;
        }

        public IEnumerable<T> CreateList<T>(IConfiguration config) where T : IModule
        {
            return config.GetChildren().Select(Create<T>);
        }
    }
}
