using System.Data;
using System.Data.Common;
using System.Globalization;
using ConsoleAppFramework;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Console;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook.Database
{
    [RegisterCommands("db")]
    [UsedImplicitly]
    public class DataFixupCommands : MlrbSqliteCommandBase
    {
        public DataFixupCommands(ILogger logger, ICliDisplay cliDisplay, ISqliteHelper sqliteHelper) :
            base(logger, cliDisplay, sqliteHelper)
        {
        }

        /// <summary>
        ///     Migrates SimpleRangeEventRow IDs to ULID format in the database.
        /// </summary>
        /// <param name="ct">A token to monitor for cancellation requests.</param>
        /// <returns>An integer representing the success or failure of the operation.</returns>
        [Command("fix-pk-ids")]
        [UsedImplicitly]
        public async Task<int> MigrateNanoidsToUlids(CancellationToken ct)
        {
            await using SqliteConnection conn = await SqliteHelper.GetDatabaseConnectionAsync(ct).ConfigureAwait(false);
            await using DbTransaction trans = await conn.BeginTransactionAsync(ct).ConfigureAwait(false);
            Logger.Information("Fixing up IDs on {databaseName}.", conn.DataSource);
            CliDisplay.PrintCommandHeader($"Fixing up IDs on {conn.DataSource}");

            Result t1 = await ChangeSimpleRangeEventIdsToUlids(conn, trans, ct).ConfigureAwait(false);
            if (t1.IsFailed)
            {
                CliDisplay.PrintFailure("There were issues fixing up ids on the Range Events.");
                Logger.Warning("Could not update the IDs on range events. " + t1.Errors[0]);
                await trans.RollbackAsync(ct).ConfigureAwait(false);

                return ReturnCodes.FAILURE;
            }

            Result t2 = await ChangeFitFileIdsToUlids(conn, trans, ct).ConfigureAwait(false);

            if (t2.IsFailed)
            {
                await trans.RollbackAsync(ct).ConfigureAwait(false);
                CliDisplay.PrintFailure("There were issues fixing up ids on the Fit Files.");
                Logger.Warning("Could not update the IDs on range events. " + t2.Errors[0]);

                return ReturnCodes.FAILURE;
            }

            await trans.CommitAsync(ct).ConfigureAwait(false);
            Logger.Information("Finished with the ID fixup.");
            CliDisplay.PrintSuccess("Successfully updated IDs.");

            return ReturnCodes.SUCCESS;
        }


        /// <summary>
        ///     Generate the <c cref="MlrbId" /> using the timestamp from the FitFile name.  This is because the FitFile name
        ///     contains a timestamp in the format of "MM-dd-yyyy_HH-mm-ss".  By using this timestamp, we can ensure that the
        ///     generated ID is consistent and can be traced back to the original file.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="trans"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        async Task<Result> ChangeFitFileIdsToUlids(SqliteConnection conn,
            IDbTransaction trans,
            CancellationToken ct = default)
        {
            const string SQL = "SELECT Id, FileName FROM FitFiles;";
            const string UPDATE = "UPDATE main.FitFiles SET Id = @newId WHERE Id = @oldId";

            Logger.Information("Converting any IDs on FitFiles.");
            IEnumerable<FitFileRow> rows =
                await conn.QueryAsync<FitFileRow>(SQL, transaction: trans).ConfigureAwait(false);
            var idMap = rows
                .ToDictionary<FitFileRow, string, string>(row => row.Id,
                    row => MlrbId.FromFitFile(Path.Combine("C:\\Temp", row.FileName)));
            Logger.Information("It seems that there are {count} IDs that need to be converted in FitFiles.",
                idMap.Count);

            var convertedCount = 0;
            foreach (KeyValuePair<string, string> kvp in idMap)
            {
                Logger.Verbose($"Converting FitFile ID {kvp.Key} to {kvp.Value}");
                var cmd = new DapperCommand(UPDATE, new { oldId = kvp.Key, newId = kvp.Value });
                int i = await cmd.ExecuteAsync(conn, trans, ct).ConfigureAwait(false);
                if (i < 1)
                {
                    Logger.Warning("Did not convert old ID {oldId}  to new ID {newId}", kvp.Key, kvp.Value);
                }
                else
                {
                    convertedCount++;
                }
            }

            Logger.Information("Update {update} IDs.", convertedCount);

            return Result.Ok();
        }

        /// <summary>
        ///     If we don't have a valid Ulid for the ID, then create a new one.  Use the date of the event as
        ///     part of the ID generation.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="trans"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        async Task<Result> ChangeSimpleRangeEventIdsToUlids(SqliteConnection conn,
            IDbTransaction trans,
            CancellationToken ct = default)
        {
            Logger.Information("Updating IDs on SimpleRangeEvents.");
            const string SQL = "SELECT Id, EventDate FROM SimpleRangeEvents;";
            const string UPDATE = "UPDATE main.SimpleRangeEvents SET Id = @newId WHERE Id = @oldId";

            IEnumerable<SimpleRangeEventRow> rows =
                await conn.QueryAsync<SimpleRangeEventRow>(SQL, transaction: trans).ConfigureAwait(false);
            var idMap = rows
                .ToDictionary<SimpleRangeEventRow, string, string>(simpleRangeEventRow => simpleRangeEventRow.Id,
                    simpleRangeEventRow => MlrbId.From(simpleRangeEventRow.EventDateTime));
            Logger.Information("It seems that there are {count} IDs that need to be converted.", idMap.Count);

            var convertedCount = 0;
            foreach (KeyValuePair<string, string> kvp in idMap)
            {
                Logger.Verbose($"Converting SimpleRangeEvent ID {kvp.Key} to {kvp.Value}");
                var cmd = new DapperCommand(UPDATE, new { oldId = kvp.Key, newId = kvp.Value });
                int i = await cmd.ExecuteAsync(conn, trans, ct).ConfigureAwait(false);
                if (i < 1)
                {
                    Logger.Warning("Did not convert old ID {oldId}  to new ID {newId}", kvp.Key, kvp.Value);
                }
                else
                {
                    convertedCount++;
                }
            }

            Logger.Information("Update {update} IDs.", convertedCount);

            return Result.Ok();
        }

        public sealed record FitFileRow(string Id, string FileName)
        {
            const string FitFileDateFormat = "MM-dd-yyyy_HH-mm-ss";

            public DateTime FitFileTime
            {
                get
                {
                    // [TO20260521] Ensure we handle both Windows and Linux separators.
                    string normalizedFileName = FileName.Replace('\\', '/');
                    string withoutExtension = Path.GetFileNameWithoutExtension(normalizedFileName);

                    CultureInfo culture = CultureInfo.InvariantCulture;

                    var timestamp = DateTime.ParseExact(
                        withoutExtension,
                        FitFileDateFormat,
                        culture,
                        DateTimeStyles.AssumeLocal // ensures Kind == Local
                    );

                    return timestamp;
                }
            }
        }

        public sealed record SimpleRangeEventRow(string Id, string EventDate)
        {
            public DateTime EventDateTime => DateTime.Parse(EventDate).ToLocalTime();
        }
    }
}
