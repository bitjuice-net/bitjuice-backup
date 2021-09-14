using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using BitJuice.Backup.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BitJuice.Backup.Infrastructure
{
    public class ModuleRepository : IModuleRepository
    {
        private readonly ILogger<ModuleRepository> logger;
        private readonly IConfiguration configuration;
        private readonly Dictionary<string, Type> moduleInfos = new(StringComparer.InvariantCultureIgnoreCase);
        
        public ModuleRepository(ILogger<ModuleRepository> logger, IConfiguration configuration)
        {
            this.logger = logger;
            this.configuration = configuration;

            LoadModules();
        }

        public Type GetModuleInfo<T>(string moduleName) where T : IModule
        {
            if (moduleInfos.TryGetValue(moduleName, out var type))
                return type;
            throw new Exception($"Missing module '{moduleName}'.");
        }

        private void LoadModules()
        {
            foreach (var section in configuration.GetSection("modules").GetChildren())
            {
                try
                {
                    AddAssembly(Assembly.Load(new AssemblyName(section.Value)));
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, $"Cannot load assembly '{section.Value}'");
                }
            }
        }

        private void AddAssembly(Assembly assembly)
        {
            var types = assembly.GetTypes().Where(i => typeof(IModule).IsAssignableFrom(i)).Where(i => !i.IsAbstract).ToList();
            foreach (var type in types) 
                AddModule(GetName(type), type);
        }

        private void AddModule(string moduleName, Type moduleType)
        {
            if (moduleInfos.TryGetValue(moduleName, out var existing))
                throw new Exception($"Cannot insert module '{moduleName}' of type 'type'. Module with the same name already exists of type '{existing}'");
            logger.LogInformation($"Registering module '{moduleName}' with type '{moduleType.FullName}'");
            moduleInfos.Add(moduleName, moduleType);
        }

        private string GetName(Type moduleType)
        {
            var attribute = moduleType.GetCustomAttribute<ModuleNameAttribute>();
            if (attribute != null)
                return attribute.Name;
            return ToSnakeCase(moduleType.Name);
        }

        private static string ToSnakeCase(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));
            
            if (text.Length < 2)
                return text;

            var sb = new StringBuilder();
            sb.Append(char.ToLowerInvariant(text[0]));
            for (var i = 1; i < text.Length; ++i)
            {
                var c = text[i];
                if (char.IsUpper(c))
                {
                    sb.Append('-');
                    sb.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}
