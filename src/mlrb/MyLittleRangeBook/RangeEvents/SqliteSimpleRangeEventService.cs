using System.Globalization;
using CsvHelper;
using Fisher;
using Fisher.Linq;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook.RangeEvents
{
    /// <summary>
    ///     Provides SQLite-specific implementation for managing SimpleRangeEvent data.
    /// </summary>
    /// <remarks>
    ///     This service offers functionalities for creating, updating, deleting, and retrieving
    ///     SimpleRangeEvent records from an SQLite database. It interacts with the database using
    ///     provided connection and transaction parameters, supporting asynchronous operations.
    /// </remarks>
    [Obsolete("Using Fisher now.", true)]
    public class SqliteSimpleRangeEventService : ISimpleRangeEventService
    {
        readonly ILogger          _logger;
        readonly IDocumentSession _session;

        public SqliteSimpleRangeEventService(ILogger       logger, IDocumentSession session)
        {
            _session      = session;
            _logger       = logger;
        }

        [Obsolete("This method is deprecated and will be removed in a future version.", true)]
        public Task<Result> DeleteAsync(DapperCommandContext context, SimpleRangeEvent simpleRangeEvent) =>
            throw new NotImplementedException();

        public Task<Result> DeleteAsync(SimpleRangeEvent simpleRangeEvent, CancellationToken cancellationToken = default) => DeleteAsync(simpleRangeEvent.Id!, cancellationToken);

        public async Task<Result> DeleteAsync(Guid simpleRangeEventId, CancellationToken cancellationToken = default)
        {
            Result result;
            if (await _session.CheckExistsAsync<SimpleRangeEvent>((Guid) simpleRangeEventId))
            {
                _session.Delete<SimpleRangeEvent>((Guid) simpleRangeEventId);
                await _session.SaveChangesAsync();
                result = Result.Ok().WithSuccess("Deleted the simple range event.");
            }
            else
            {
                result = Result.Ok().WithSuccess("The simple range event didn't exist.");
            }

            return result;
        }

        public Task<Result<SimpleRangeEvent>> GetAsync(DapperCommandContext context, Guid simpleRangeEventId) => throw new NotImplementedException();

        [Obsolete("This method is deprecated and will be removed in a future version.", true)]
        public async Task<Result> ExportToCsv(DapperCommandContext context, string csvFileName)
        {
            try
            {
                if (File.Exists(csvFileName))
                {
                    File.Delete(csvFileName);
                }

                IEnumerable<SimpleRangeEvent> rangeEvents = await Commands.s_selectAll
                                                                          .QueryAsync<SimpleRangeEvent>(context)
                                                                          .ConfigureAwait(false);

                await using StreamWriter writer = new(csvFileName);
                await using CsvWriter    csv    = new(writer, CultureInfo.InvariantCulture);

                await csv.WriteRecordsAsync(rangeEvents.OrderBy(x => x.EventDate), context.CancellationToken)
                         .ConfigureAwait(false);

                SimpleRangeEventsExportedToCsvSuccess success = new(csvFileName, rangeEvents.Count());
                return Result.Ok().WithSuccess(success);
            }
            catch (Exception ex)
            {
                SimpleRangeEventsExportToCsvError error = new(csvFileName, ex);
                error.CausedBy(ex);

                return Result.Fail(error);
            }
        }

        [Obsolete("This method is deprecated and will be removed in a future version.", true)]
        public async Task<Result<IEnumerable<string>>> GetAmmoDescriptions(DapperCommandContext context)
        {
            try
            {
                IEnumerable<string> list = await Commands.s_ammoDescriptionCommand.QueryAsync<string>(context)
                                                         .ConfigureAwait(false);
                return Result.Ok(list);
            }
            catch (Exception ex)
            {
                Error err = ex.ToError();
                return Result.Fail<IEnumerable<string>>(err);
            }
        }

        public Task<Result<IEnumerable<string>>> GetAmmoDescriptions(CancellationToken cancellationToken = default) => throw new NotImplementedException();

        [Obsolete("This method is deprecated and will be removed in a future version.", true)]
        public async Task<Result<IEnumerable<string>>> GetRangeNames(DapperCommandContext context)
        {
            try
            {
                IEnumerable<string> list = await Commands.s_rangeNamesCommand.QueryAsync<string>(context)
                                                         .ConfigureAwait(false);
                return Result.Ok(list);
            }
            catch (Exception ex)
            {
                Error err = ex.ToError();
                return Result.Fail<IEnumerable<string>>(err);
            }
        }

        public Task<Result<IEnumerable<string>>> GetRangeNames(CancellationToken cancellationToken = default) => throw new NotImplementedException();

        [Obsolete("This method is deprecated and will be removed in a future version.", true)]
        public async Task<Result<SimpleRangeEvent>> GetAsync(DapperCommandContext context, MlrbId simpleRangeEventId)
        {
            Result<SimpleRangeEvent?> result;

            if (await _session.CheckExistsAsync<SimpleRangeEvent>((Guid) simpleRangeEventId))
            {
                SimpleRangeEvent? sre = await _session.LoadAsync<SimpleRangeEvent>((Guid) simpleRangeEventId);
                result = Result.Ok(sre);
            }
            else
            {
                _logger.Debug("Could not find a Fisher document for simple range event.");
                result = new Result<SimpleRangeEvent?>().WithSuccess("Could not find the simple range event.");
            }

            return result;
        }

        public Task<Result<SimpleRangeEvent>> GetAsync(Guid simpleRangeEventId, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();



        [Obsolete("This method is deprecated and will be removed in a future version.", true)]
        public async Task<Result<IEnumerable<SimpleRangeEvent>>> GetSimpleRangeEventsAsync(DapperCommandContext ctx)
        {
            Result<IEnumerable<SimpleRangeEvent>> result;

            try
            {
                IReadOnlyList<SimpleRangeEvent> list = await _session.Query<SimpleRangeEvent>()
                                                                          .OrderBy(x => x.EventDate)
                                                                          .ToListAsync();
                result = new Result<IEnumerable<SimpleRangeEvent>>().WithValue(list);
            }
            catch (Exception e1)
            {
                _logger.Warning(e1, "An error occurred while fetching simple range events.");
                result = new Result<IEnumerable<SimpleRangeEvent>>().WithError(e1.ToError());
            }

            return result;
        }

        public async Task<Result<Guid>> UpsertAsync(SimpleRangeEvent  simpleRangeEvent,
                                                CancellationToken cancellationToken = default)
        {
            Result<Guid> result = new();
            simpleRangeEvent.Modified = DateTimeOffset.UtcNow;
            try
            {
                _session.Store(simpleRangeEvent);
                await _session.SaveChangesAsync(cancellationToken);
                result.Reasons.Add(new Success("Saved simple range event to document store."));
            }
            catch (Exception e2)
            {
                result.Reasons.Add(e2.ToError().Enrich(simpleRangeEvent.Id!));
                // [TO20260821] We don't consider this an error yet.
                _logger.Warning(e2, "Failed to save simple range event document store.");
            }

            return result;
        }

        [Obsolete("This method is deprecated and will be removed in a future version.", true)]
        public Task<Result<Guid>> UpsertAsync(DapperCommandContext context,
                                                      SimpleRangeEvent     simpleRangeEvent) =>
            throw new NotImplementedException();

        public async Task<Result<IEnumerable<SimpleRangeEvent>>> GetSimpleRangeEventsAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                IReadOnlyList<SimpleRangeEvent> rangeEvents =
                    await _session.Query<SimpleRangeEvent>().ToListAsync(cancellationToken);
                return Result.Ok<IEnumerable<SimpleRangeEvent>>(rangeEvents);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "An error occurred while fetching simple range events.");
                return Result.Fail<IEnumerable<SimpleRangeEvent>>(ex.ToError());
            }
        }

        public async Task<Result> ExportToCsv(string csvFileName, CancellationToken cancellationToken = default)
        {
            try
            {
                if (File.Exists(csvFileName))
                {
                    File.Delete(csvFileName);
                }

                IReadOnlyList<SimpleRangeEvent> rangeEvents = await _session.Query<SimpleRangeEvent>()
                                                                            .OrderBy(x => x.EventDate)
                                                                            .ThenBy(x => x.FirearmName)
                                                                            .ToListAsync(cancellationToken);
                await using StreamWriter writer = new(csvFileName);
                await using CsvWriter    csv    = new(writer, CultureInfo.InvariantCulture);

                await csv.WriteRecordsAsync(rangeEvents.OrderBy(x => x.EventDate), cancellationToken)
                         .ConfigureAwait(false);

                SimpleRangeEventsExportedToCsvSuccess success = new(csvFileName, rangeEvents.Count());
                return Result.Ok().WithSuccess(success);
            }
            catch (OperationCanceledException)
            {
                throw;
            }

            catch (Exception ex)
            {
                SimpleRangeEventsExportToCsvError error = new(csvFileName, ex);
                error.CausedBy(ex);

                return Result.Fail(error);
            }
        }

        static class Commands
        {
            const string FIREARM_NAMES_SQL = """
                                             SELECT DISTINCT TRIM(simple_range_events.firearm_name) AS fn
                                             FROM simple_range_events
                                             WHERE length(trim(firearm_name)) > 0
                                             ORDER BY firearm_name COLLATE NOCASE;
                                             """;

            const string RANGE_NAMES_SQL = """
                                           SELECT DISTINCT TRIM(simple_range_events.range_name) AS rn
                                           FROM simple_range_events
                                           WHERE length(trim(range_name)) > 0
                                           ORDER BY range_name COLLATE NOCASE;
                                           """;

            const string AMMO_DESCRIPTIONS_SQL = """
                                                 SELECT DISTINCT TRIM(simple_range_events.ammo_description) AS ad
                                                 FROM simple_range_events
                                                 WHERE length(trim(ammo_description)) > 0
                                                 ORDER BY ammo_description COLLATE NOCASE;
                                                 """;

            const string UPSERT_SQL = """
                                      INSERT INTO simple_range_events (id, event_date, firearm_name, range_name, rounds_fired, ammo_description, notes, created, modified)
                                      VALUES (@Id, @EventDate, @FirearmName, @RangeName, @RoundsFired, @AmmoDescription, @Notes, @Created, @Modified)
                                      ON CONFLICT(id) DO UPDATE SET
                                        event_date = excluded.event_date,
                                        firearm_name = excluded.firearm_name,
                                        range_name = excluded.range_name,
                                        rounds_fired = excluded.rounds_fired,
                                        ammo_description = excluded.ammo_description,
                                        notes = excluded.notes,
                                        modified = excluded.modified
                                      RETURNING row_id;
                                      """;

            const string SELECT_BY_ID_SQL = """
                                            SELECT
                                                row_id AS RowId,
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
                                            WHERE id =@Id;
                                            """;

            const string SELECT_SQL = """
                                      SELECT
                                          row_id AS RowId,
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
                                      ORDER BY event_date, firearm_name, range_name;
                                      """;

            const string DELETE_SQL = "DELETE FROM simple_range_events WHERE id = @Id;";

            internal static readonly DapperCommand s_upsertCommand          = new(UPSERT_SQL);
            internal static readonly DapperCommand s_deleteCommand          = new(DELETE_SQL);
            internal static readonly DapperCommand s_selectAll              = new(SELECT_SQL);
            internal static readonly DapperCommand s_selectById             = new(SELECT_BY_ID_SQL);
            internal static readonly DapperCommand s_ammoDescriptionCommand = new(AMMO_DESCRIPTIONS_SQL);
            internal static readonly DapperCommand s_rangeNamesCommand      = new(RANGE_NAMES_SQL);
        }
    }
}