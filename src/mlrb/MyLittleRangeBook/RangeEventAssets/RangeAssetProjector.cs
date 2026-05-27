using System.Data.Common;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;
using static MyLittleRangeBook.RangeEventAssets.RangeAssetAggregate;

namespace MyLittleRangeBook.RangeEventAssets
{
    public readonly record struct RangeAssetProjectionContext(
        SqliteConnection Connection,
        DbTransaction? Transaction,
        MlrbId StreamId,
        IReadOnlyList<IDomainEvent> PendingEvents,
        CancellationToken CancellationToken = default)
    {
        public TDomainEvent GetEvent<TDomainEvent>() where TDomainEvent : IDomainEvent
        {
            return (TDomainEvent)PendingEvents.First(@event => @event is TDomainEvent);
        }
    }


    /// <summary>
    ///     Defines functionality for projecting domain events related to file imports into a storage system.
    /// </summary>
    public interface IRangeAssetProjector
    {
        Task<Result> ProjectAsync(RangeAssetProjectionContext context);
    }

    class SqliteRangeAssetProjector : IRangeAssetProjector
    {
        readonly ILogger _logger;
        readonly ISqliteHelper _sqliteHelper;

        public SqliteRangeAssetProjector(ILogger logger, ISqliteHelper sqliteHelper)
        {
            _logger = logger;
            _sqliteHelper = sqliteHelper;
        }

        public async Task<Result> ProjectAsync(RangeAssetProjectionContext context)
        {
            if (context.PendingEvents.Count == 0)
            {
                return Result.Ok();
            }


            // TODO [TO20260527] Depending on the type of asset, extract data and update tables.
            return await ProjectRangeAssetAssociateWithRangeEvent(context);
        }

        async Task<Result> ProjectRangeAssetAssociateWithRangeEvent(RangeAssetProjectionContext context)
        {
            const string errorMessage =
                "The association between the range event and range asset file was not created as expected.";
            const string sql = """
                               INSERT INTO SimpleRangeEvent_RangeAssetFiles (SimpleRangeEventId, RangeAssetFilesId) 
                               VALUES (@RangeEventId, @RangeAssetId)
                               """;
            Error errr = new Error(errorMessage).Enrich(context.StreamId);

            RangeAssetAssociateWithRangeEvent @event;
            try
            {
                @event = context.GetEvent<RangeAssetAssociateWithRangeEvent>();
            }
            catch (InvalidOperationException)
            {
                // [TO20260527] No event in the collection; nothing to do.
                _logger.Verbose("RangeAsset has not been associated with any RangeEvent. Nothing to do.");

                return Result.Ok();
            }
            catch (Exception e)
            {
                return errr.CausedBy(e);
            }

            var p = new { @event.RangeEventId, RangeAssetId = context.StreamId };
            var c = new DapperCommand(sql, p);
            int r = await c.ExecuteAsync(context.Connection, context.Transaction, context.CancellationToken)
                .ConfigureAwait(false);

            return r == 1 ? Result.Ok() : Result.Fail(errr);
        }
    }
}
