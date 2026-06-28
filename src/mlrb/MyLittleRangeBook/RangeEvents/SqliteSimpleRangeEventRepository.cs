using Microsoft.Extensions.DependencyInjection;
using MyLittleRangeBook.EventSourcing;
using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;
using static MyLittleRangeBook.Persistence.Sqlite.SqliteHelperExtensions;

namespace MyLittleRangeBook.RangeEvents
{
    /// <summary>
    ///     Represents a repository implementation for managing simple range events using SQLite as the persistence layer.
    /// </summary>
    public class SqliteSimpleRangeEventRepository : ISimpleRangeEventRepository
    {
        readonly IFirearmAggregateRepository _faRepo;

        readonly ISimpleRangeEventService    _simpleRangeEventService;


        public SqliteSimpleRangeEventRepository(ISqliteHelper sqliteHelper,
                                                [FromKeyedServices(DI_KEYS_SQLITE)]
                                                ISimpleRangeEventService simpleRangeEventService,

                                                IFirearmAggregateRepository faRepo)
        {
            _simpleRangeEventService = simpleRangeEventService;
            _faRepo                  = faRepo;
        }

        public async Task<Result<SimpleRangeEvent>> GetAsync(DapperCommandContext context, string id)
        {
            Result<SimpleRangeEvent> getResult = await _simpleRangeEventService.GetAsync(context, id);
            return getResult.Value;
        }

        public async Task<Result<IEnumerable<SimpleRangeEvent>>> GetSimpleRangeEventsAsync(DapperCommandContext ctx)
        {
            try
            {
                Result<IEnumerable<SimpleRangeEvent>> result =
                    await _simpleRangeEventService.GetSimpleRangeEventsAsync(ctx).ConfigureAwait(false);
                if (!result.IsFailed)
                {
                    return result;
                }

                return Result.Fail(result.Errors);
            }
            catch (Exception e)
            {
                Error err = new Error("Failed to retrieve simple range events.").CausedBy(e);

                return Result.Fail(err);
            }
        }

        public async Task<Result> DeleteAsync(DapperCommandContext context, SimpleRangeEvent simpleRangeEvent)
        {
            try
            {
                return await _simpleRangeEventService.DeleteAsync(context, simpleRangeEvent)
                                                     .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Error? err = new Error("Failed to delete simple range event.")
                            .Enrich(simpleRangeEvent.Id!, simpleRangeEvent.RowId)
                            .CausedBy(e);

                return Result.Fail(err);
            }
        }

        /// <summary>
        ///     Inserts or updates a SimpleRangeEvent in the database. If the event does not exist, it will be created;
        ///     otherwise, it will be updated with the provided data. Will also append to the event stream for the firearm.
        /// </summary>
        /// <param name="context">
        ///     The Dapper command context encapsulating the database connection, transaction, and other related
        ///     parameters.
        /// </param>
        /// <param name="simpleRangeEvent">The SimpleRangeEvent object to be inserted or updated in the database.</param>
        /// <returns>
        ///     A result object containing the ID of the record that was inserted or updated,
        ///     or an error if the operation failed.
        /// </returns>
        public async Task<Result<MlrbId>> UpsertAsync(DapperCommandContext context, SimpleRangeEvent simpleRangeEvent)
        {
            List<IReason> reasons = [];

            Result<MlrbId> r1 = await UpsertSimpleRangeEventTable(context, simpleRangeEvent).ConfigureAwait(false);
            reasons.AddRange(r1.Reasons);

            return new Result().WithReasons(reasons).ToResult<MlrbId>(simpleRangeEvent.Id!);
        }



        static void AssociateWithFirearm(FirearmAggregate fa, SimpleRangeEvent sre) =>
            // TODO [TO20260626] - Capture the note and ammo description as metadata.
            fa.AssociateWithSimpleRangeEvent(sre.Id!, sre.EventDate);

        async Task<Result<MlrbId>> UpsertSimpleRangeEventTable(DapperCommandContext context,
                                                               SimpleRangeEvent     simpleRangeEvent)
        {
            Result<MlrbId> upsertResult = await _simpleRangeEventService
                                               .UpsertAsync(context, simpleRangeEvent)
                                               .ConfigureAwait(false);
            if (upsertResult.IsFailed || upsertResult.Value == MlrbId.Empty)
            {
                return Result.Fail(upsertResult.Errors);
            }

            MlrbId id = upsertResult.Value;
            Success? success = new Success("Upserted the simple_range_events table.")
               .WithMetadata("RowId", id);

            return Result.Ok().WithSuccess(success);
        }
    }
}