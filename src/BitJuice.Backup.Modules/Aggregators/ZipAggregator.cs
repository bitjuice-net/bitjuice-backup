using System;
using BitJuice.Backup.Infrastructure;

namespace BitJuice.Backup.Modules.Aggregators
{
    [ModuleName("zip-aggregator")]
    public class ZipAggregator: ArchiveAggregator<ZipBuilder>
    {
        public ZipAggregator(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
