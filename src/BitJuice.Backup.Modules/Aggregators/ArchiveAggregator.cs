using System;
using System.Collections.Generic;
using BitJuice.Backup.Infrastructure;
using BitJuice.Backup.Model;
using Microsoft.Extensions.DependencyInjection;

namespace BitJuice.Backup.Modules.Aggregators
{
    public abstract class ArchiveAggregator<TBuilder> : ModuleBase<ArchiveConfig>, IAggregator where TBuilder : ArchiveBuilder
    {
        private readonly IServiceProvider serviceProvider;

        protected ArchiveAggregator(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IEnumerable<IDataItem> Aggregate(IEnumerable<IDataItem> items)
        {
            var builder = ActivatorUtilities.CreateInstance<TBuilder>(serviceProvider, items);
            yield return new ArchiveDataItem(Config.Filename, builder);
        }
    }
}