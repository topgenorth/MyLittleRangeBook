using System.Globalization;
using CsvHelper;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook.RangeEvents
{
    public sealed class SimpleRangeEventsExportedToCsvSuccess : Success
    {
        public SimpleRangeEventsExportedToCsvSuccess(string csvFileName, int rowCount) :
            base($"Exported {rowCount} simple range event(s) to CSV file `{csvFileName}`.")
        {
            Metadata.Add(nameof(csvFileName), csvFileName);
            Metadata.Add(nameof(rowCount),    rowCount);
        }
    }

    public sealed class SimpleRangeEventsExportToCsvError : Error
    {
        public SimpleRangeEventsExportToCsvError(string csvFileName, Exception exception) :
            base($"Could not export simple range events to CSV file `{csvFileName}`.")
        {
            Metadata.Add(nameof(csvFileName), csvFileName);
            CausedBy(exception);
        }
    }


    public class DeletedEventStreamReason(MlrbId firearmId)
        : Success($"The event stream was deleted (ID: {firearmId})")
    {
        public MlrbId FirearmId = firearmId;
    }

    /// <summary>
    ///     Provides SQLite-specific implementation for managing SimpleRangeEvent data.
    /// </summary>
    /// <remarks>
    ///     This service offers functionalities for creating, updating, deleting, and retrieving
    ///     SimpleRangeEvent records from an SQLite database. It interacts with the database using
    ///     provided connection and transaction parameters, supporting asynchronous operations.
    /// </remarks>
    public class SqliteSimpleRangeEventService : ISimpleRangeEventService
    {
        public async Task<Result> DeleteAsync(DapperCommandContext context, SimpleRangeEvent simpleRangeEvent)
        {
            try
            {
                // TODO Delete any firearms that might be associated.

                var                  p   = new { Id = simpleRangeEvent.Id! };
                DapperCommandContext ctx = context with { Arguments = p };
                int result = await Commands.s_deleteCommand
                                           .ExecuteAsync(ctx)
                                           .ConfigureAwait(false);
                return Result.Ok();
            }
            catch (Exception e)
            {
                Error err = e.ToError().Enrich(simpleRangeEvent.Id!);
                return new Result().WithError(err);
            }
        }


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

        public async Task<Result<IEnumerable<string>>> GetRangeNames(DapperCommandContext context)
        {
            try
            {
                var list= await Commands.s_rangeNamesCommand.QueryAsync<string>(context)
                                                         .ConfigureAwait(false);
                return Result.Ok(list);
            }
            catch (Exception ex)
            {
                Error err = ex.ToError();
                return Result.Fail<IEnumerable<string>>(err);
            }
        }

        public async Task<Result<SimpleRangeEvent>> GetAsync(DapperCommandContext context, MlrbId simpleRangeEventId)
        {
            try
            {
                DapperCommandContext ctx = context with { Arguments = new { Id = simpleRangeEventId } };
                SimpleRangeEvent sre = await Commands.s_selectById.QuerySingleAsync<SimpleRangeEvent>(ctx)
                                                     .ConfigureAwait(false);
                return sre;
            }
            catch (Exception e)
            {
                Error err = e.ToError().Enrich(simpleRangeEventId);
                return Result.Fail(err);
            }
        }

        public async Task<Result<IEnumerable<SimpleRangeEvent>>> GetSimpleRangeEventsAsync(DapperCommandContext ctx)
        {
            try
            {
                IEnumerable<SimpleRangeEvent> rangeEvents = await Commands.s_selectAll
                                                                          .QueryAsync<SimpleRangeEvent>(ctx)
                                                                          .ConfigureAwait(false);

                return Result.Ok(rangeEvents);
            }
            catch (Exception e)
            {
                Error err = new($"Could not retrieve SimpleRangeEvents from database: {e.Message}");
                err.CausedBy(e);

                return Result.Fail(err);
            }
        }

        public async Task<Result<MlrbId>> UpsertAsync(DapperCommandContext context,
                                                      SimpleRangeEvent     simpleRangeEvent)
        {
            simpleRangeEvent.Modified = DateTimeOffset.UtcNow;
            if (simpleRangeEvent.RowId == null)
            {
                simpleRangeEvent.Id = MlrbId.From(simpleRangeEvent.EventDate);
            }

            try
            {
                var p = new
                        {
                            Id = simpleRangeEvent.Id!,
                            simpleRangeEvent.EventDate,
                            simpleRangeEvent.FirearmName,
                            simpleRangeEvent.RangeName,
                            simpleRangeEvent.RoundsFired,
                            simpleRangeEvent.AmmoDescription,
                            simpleRangeEvent.Notes,
                            simpleRangeEvent.Created,
                            simpleRangeEvent.Modified,
                        };
                DapperCommandContext ctx = context with { Arguments = p };
                long result = await Commands.s_upsertCommand.ExecuteScalarAsync<long>(ctx).ConfigureAwait(false);

                simpleRangeEvent.RowId = result;


                Success reason = new($"SimpleRangeEvent `{simpleRangeEvent.Id}` saved.");
                reason.Enrich(simpleRangeEvent.Id!, simpleRangeEvent.RowId);
                return Result.Ok((MlrbId)simpleRangeEvent.Id!).WithSuccess(reason);
            }
            catch (Exception e)
            {
                Error err = e.ToError().Enrich(simpleRangeEvent.Id!, simpleRangeEvent.RowId);

                return Result.Fail(err);
            }
        }

        static class Commands
        {

            const string FIREARM_NAMES_SQL= """
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