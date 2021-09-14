using System;
using BitJuice.Backup.Infrastructure;

namespace BitJuice.Backup.Modules.Aggregators
{
    [ModuleName("tgz-aggregator")]
    public class TgzAggregator : ArchiveAggregator<TgzBuilder>
    {
        public TgzAggregator(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
