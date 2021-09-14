using System;
using BitJuice.Backup.Infrastructure;

namespace BitJuice.Backup.Modules.Aggregators
{
    [ModuleName("tar-aggregator")]
    public class TarAggregator : ArchiveAggregator<TarBuilder>
    {
        public TarAggregator(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}