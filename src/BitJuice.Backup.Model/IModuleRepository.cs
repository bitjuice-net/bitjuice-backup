using System;

namespace BitJuice.Backup.Model
{
    public interface IModuleRepository
    {
        Type GetModuleInfo<T>(string moduleName) where T : IModule;
    }
}