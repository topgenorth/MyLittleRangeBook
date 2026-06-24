using Dapper;
using Microsoft.Extensions.DependencyInjection;
using MyLittleRangeBook.EventSourcing;
using MyLittleRangeBook.Firearms;
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
        readonly ISqliteHelper               _sqliteHelper;


        public SqliteSimpleRangeEventRepository(ISqliteHelper sqliteHelper,
                                                [FromKeyedServices(DI_KEYS_SQLITE)]
                                                ISimpleRangeEventService simpleRangeEventService,
                                                IFirearmAggregateRepository faRepo
                                                )
        {
            _sqliteHelper                   = sqliteHelper;
            _simpleRangeEventService        = simpleRangeEventService;
            _faRepo                         = faRepo;
        }

        public async Task<Result<SimpleRangeEvent>> GetAsync(DapperCommandContext context, string id)
        {
            try
            {
                SimpleRangeEvent? sre = await context.Connection
                                                     .QueryFirstOrDefaultAsync<SimpleRangeEvent>(Commands.GetByIdSql,
                                                          new { Id = id });
                if (sre is not null)
                {
                    return Result.Ok(sre);
                }

                Error err = new Error("Could not find range event " + id + ".").Enrich(id);
                return Result.Fail(err);
            }
            catch (Exception ex)
            {
                Error err = new Error(ex.Message).CausedBy(ex).Enrich(id);

                return Result.Fail(err);
            }
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
        public async Task<Result<long>> UpsertAsync(DapperCommandContext context, SimpleRangeEvent simpleRangeEvent)
        {
            Result<long?> sreResult = await _simpleRangeEventService
                                           .UpsertAsync(context, simpleRangeEvent)
                                           .ConfigureAwait(false);
            if (sreResult.IsFailed || sreResult.Value is null)
            {
                return Result.Fail(sreResult.Errors);
            }
            return Result.Ok(sreResult.Value.Value);
        }

        static class Commands
        {
            internal const string GetByIdSql = """
                                               SELECT row_id AS RowId,
                                                      id AS Id,
                                                      event_date AS EventDate,
                                                      firearm_name AS FirearmName,
                                                      range_name AS RangeName,
                                                      rounds_fired AS RoundsFired,
                                                      ammo_description AS AmmoDescription,
                                                      notes AS Notes,
                                                      created AS Created,
                                                      modified AS Modified
                                               FROM simple_range_events
                                               WHERE id=@Id;
                                               """;
        }
    }
}