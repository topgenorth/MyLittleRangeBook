using ConsoleAppFramework;
using JetBrains.Annotations;
using MyLittleRangeBook.Console;
using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook
{
    [RegisterCommands("firearms")]
    public class UpdateFirearmsFromRangeEventsCommand : MlrbFirearmsCommandBase
    {
        public UpdateFirearmsFromRangeEventsCommand(ILogger logger, ICliDisplay display, ISqliteHelper sqliteHelper,
            IFirearmsService firearmsService, IFirearmAggregateRepository firearmAggregateRepo
            ) : base(logger, display, sqliteHelper, firearmsService, firearmAggregateRepo)
        {

        }

        /// <summary>
        ///     This is a maintenance task. It will update the Firearms table based on what is in the SimpleRangeEvents table.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Command("import-from-range-events"), UsedImplicitly]
        public async Task<int> ImportFirearmsFromRangeEvents(CancellationToken cancellationToken = default)
        {
            CliDisplay.PrintCommandHeader("Import new firearms from range events.");

            var returnCode = -1;
            IEnumerable<NewFirearmNameRow> firearms;
            try
            {
                await using var scopedConn = await SqliteHelper.GetScopedDatabaseConnectionAsync(cancellationToken)
                    .ConfigureAwait(false);
                await using var trans =
                    await scopedConn.Connection.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
                var ctx = new DapperCommandContext(scopedConn, trans, cancellationToken);
                firearms = await Commands
                    .GetNewFirearmsFromRangeEvents
                    .QueryAsync<NewFirearmNameRow>(ctx)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                CliDisplay.PrintFailure("something bad happened trying to figure out new firearms.");
                Logger.Error(ex, "Failed to update firearms from range events");
                returnCode = ReturnCodes.FAILURE;

                goto ExitFunction;
            }

            foreach (var row in firearms)
            {
                var fa = FirearmAggregate.New(row.FirearmName,  row.CreatedUtc);
                var associationList = new List<SimpleRangeEventFirearmRow>();

                var rangeEvents = await GetSimpleRangeEventsForFirearmName(row.FirearmName, cancellationToken).ConfigureAwait(false);
                foreach (var rangeEvent in rangeEvents)
                {
                    fa.AssociateWithSimpleRangeEvent(rangeEvent.SimpleRangeEventId, rangeEvent.CreatedUtc);
                    fa.MoreRoundsFired(rangeEvent.RoundsFired , rangeEvent.CreatedUtc);
                    associationList.Add(new SimpleRangeEventFirearmRow(fa.Id, rangeEvent.SimpleRangeEventId));
                }

                fa.AppendToNotes("Imported from SimpleRangeEvents.", DateTimeOffset.UtcNow);
                var x = await FirearmAggregateRepository.SaveAsync(fa, cancellationToken).ConfigureAwait(false);


                var f = fa.ToFirearm();

                CliDisplay.PrintInfo(x.IsSuccess
                    ? $"Created {fa.Id}:{fa.Name}, {fa.RoundsFired} rounds fired."
                    : $"Failed to create  {fa.Id}:{fa.Name}, {fa.RoundsFired} rounds fired. {x.Errors[0]}");
            }

            returnCode = ReturnCodes.SUCCESS;

            ExitFunction:
            PressEnterToContinue();

            return returnCode;
        }

        async Task<IList<RangeEventForFirearmNameRow>> GetSimpleRangeEventsForFirearmName(string name, CancellationToken ct)
        {
            await using var scopedConn = await SqliteHelper.GetScopedDatabaseConnectionAsync(ct).ConfigureAwait(false);
            var ctx = new DapperCommandContext(scopedConn.Connection, CancellationToken: ct,
                Arguments: new { FirearmName = name });

            var list = await Commands.GetRangeEventsForFirearmName
                .QueryAsync<RangeEventForFirearmNameRow>(ctx)
                .ConfigureAwait(false);
            return list.ToList();
        }

        private readonly record struct NewFirearmNameRow(string FirearmName, string Created)
        {
            internal DateTimeOffset CreatedUtc => DateTimeOffset.Parse(Created, null, System.Globalization.DateTimeStyles.AssumeLocal);
        }

        private readonly record struct RangeEventForFirearmNameRow(string SimpleRangeEventId, int RoundsFired, string EventDate)
        {
            internal DateTimeOffset CreatedUtc => DateTimeOffset.Parse(EventDate, null, System.Globalization.DateTimeStyles.AssumeLocal);
        }

        private readonly record struct SimpleRangeEventFirearmRow(string FirearmId, string SimpleRangeEventId);

        static class Commands
        {

            /// <summary>
            /// Retrieves the names of firearms and their associated total rounds fired,
            /// based on range events, for firearms that are not yet present in the firearms table.
            /// The results are grouped and ordered by firearm name.
            /// </summary>
            private const string GetNewFirearmNamesFromRangeEventsSql = """
                                                                        SELECT
                                                                            s.firearm_name AS FirearmName,
                                                                            MIN(s.event_date) AS Created
                                                                        FROM simple_range_events s
                                                                        WHERE s.firearm_name NOT IN (SELECT name FROM firearms)
                                                                        GROUP BY s.firearm_name
                                                                        ORDER BY Created ASC, s.firearm_name ASC;
                                                                        """;

            private const string RangeEventsForFirearmNameSql = """
                                                                SELECT s.id as SimpleRangeEventId,
                                                                       s.rounds_fired as RoundsFired,
                                                                       s.event_date as EventDate
                                                                FROM simple_range_events s
                                                                WHERE s.firearm_name = @FirearmName AND s.rounds_fired > 0
                                                                ORDER BY s.event_date;
                                                                """;

            internal static readonly DapperCommand GetNewFirearmsFromRangeEvents =
                new(GetNewFirearmNamesFromRangeEventsSql);
            internal static readonly DapperCommand GetRangeEventsForFirearmName =
                new(RangeEventsForFirearmNameSql);
         }
    }
}