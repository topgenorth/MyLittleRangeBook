using MyLittleRangeBook.EventSourcing;

namespace MyLittleRangeBook.MlrbAssets
{
    class AssociateMlrbAssetToRangeEventProjector : IRangeAssetProjector
    {
        readonly ILogger _logger;

        public AssociateMlrbAssetToRangeEventProjector(ILogger logger)
        {
            _logger = logger;
        }

        public async Task ProjectAsync(RangeAssetProjectorContext context)
        {
            _logger.Verbose("Projecting {EventCount} events for RangeAssetImport", context.PendingEvents.Count);

        }


        public Task ProjectAsync(RangeAssetProjectorContext context, IEnumerable<IDomainEvent> domainEvents)
        {
            return ProjectAsync(context, context.PendingEvents);
        }
    }
}
