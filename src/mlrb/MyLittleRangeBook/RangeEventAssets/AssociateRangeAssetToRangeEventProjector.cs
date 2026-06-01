using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook.RangeEventAssets
{
    class AssociateRangeAssetToRangeEventProjector : IRangeAssetProjector
    {
        readonly ILogger _logger;

        public AssociateRangeAssetToRangeEventProjector(ILogger logger)
        {
            _logger = logger;
        }

        public async Task ProjectAsync(RangeAssetProjectorContext context)
        {
            _logger.Verbose("Projecting {EventCount} events for RangeAssetImport", context.PendingEvents.Count);

            Result r = await AssociateRangeAssetToRangeEvent(context);
        }

        async Task<Result> AssociateRangeAssetToRangeEvent(RangeAssetProjectorContext context)
        {
            MlrbId rangeEventId;
            try
            {
                (_, rangeEventId, _) = (RangeAssetAggregate.RangeAssetAssociateWithRangeEvent)context.PendingEvents.First(domainEvent =>
                    domainEvent is RangeAssetAggregate.RangeAssetAssociateWithRangeEvent);
            }
            catch (Exception e)
            {
                _logger.Verbose(e, "Could not find a RangeAssetAssociatedWithRangeEvent: {errorMessage}.", e.Message);
                Error err = new Error(e.Message).CausedBy(e).Enrich(context.RangeAssetId);

                return Result.Fail(err);
            }

            _logger.Verbose("Associating RangeAsset {RangeAssetId} to RangeEvent", context.RangeAssetId);

            var p = new { RangeEventId = rangeEventId, context.RangeAssetId };
            var cmd = new DapperCommand("""
                                        INSERT INTO asset_files_simple_range_events (simple_range_event_id, asset_file_id) 
                                        VALUES (@RangeEventId, @RangeAssetId)
                                        """);

            var dapperCtx =
                new DapperCommandContext(context.Connection, context.Transaction, context.CancellationToken, p);
            int r = await cmd.ExecuteAsync(dapperCtx).ConfigureAwait(false);
            if (r != 1)
            {
                return Result.Fail("Could not associate range asset to the range event.");
            }

            return Result.Ok();
        }

        public Task ProjectAsync(RangeAssetProjectorContext context, IEnumerable<IDomainEvent> domainEvents)
        {
            return ProjectAsync(context, context.PendingEvents);
        }
    }
}
