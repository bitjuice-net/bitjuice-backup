using System;
using BitJuice.Backup.Model;

namespace BitJuice.Backup.Infrastructure
{
    public class InvalidConfigException<TConfig> : Exception where TConfig : IModuleConfig
    {
        public InvalidConfigException(string message) : base($"{nameof(TConfig)}: {message}")
        {
        }
    }
}
