using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace BitJuice.Backup.Model
{
    public interface IModuleFactory
    {
        T Create<T>(IConfiguration section) where T : IModule;
        IEnumerable<T> CreateList<T>(IConfiguration config) where T : IModule;
    }
}