using System.Globalization;
using ConsoleAppFramework;
using CsvHelper;
using CsvHelper.Configuration;
using FluentResults;
using JetBrains.Annotations;
using MyLittleRangeBook.Console;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook.RangeEvents
{
    [RegisterCommands("rangeevent")]
    [UsedImplicitly]
    public class ImportRangeEventsFromCsvCommand : MlrbSqliteCommandBase
    {
        readonly ISimpleRangeEventDataProcessor _simpleRangeEventProcessor;

        public ImportRangeEventsFromCsvCommand(ILogger logger, ICliDisplay display, ISqliteHelper sqliteHelper,
                                               ISimpleRangeEventDataProcessor simpleRangeEventProcessor) :
            base(logger, display, sqliteHelper) =>
            _simpleRangeEventProcessor = simpleRangeEventProcessor;

        [Command("import-from-csv")]
        [UsedImplicitly]
        public async Task<int> ImportFromCsvFile(string file, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(file))
            {
                Logger.Error("The CSV file '{csvFileName}' was not found.", file);
                return ReturnCodes.SHOTVIEW_FILE_NOT_FOUND;
            }

            await using DapperCommandContext context = await DapperCommandContext
                                                            .NewAsync(SqliteHelper,
                                                                      cancellationToken,
                                                                      withTransaction: true)
                                                            .ConfigureAwait(false);

            try
            {
                int count = 0;
                await foreach (CsvRow sre in LoadRangeEventsFromCsv(file, cancellationToken).ConfigureAwait(false))
                {
                    Result<MlrbId> result = await _simpleRangeEventProcessor.ProcessSimpleRangeEventData(
                                                 context,
                                                 sre.FirearmName,
                                                 sre.RoundsFired,
                                                 sre.RangeName,
                                                 sre.AmmoDescription ?? string.Empty,
                                                 sre.Notes           ?? string.Empty,
                                                 DateOnly.Parse(sre.EventDate)
                                                ).ConfigureAwait(false);

                    if (result.IsSuccess)
                    {
                        count++;
                    }
                    else
                    {
                        Logger.Warning("Failed to import row {rowId}: {error}", sre.RowId,
                                       string.Join(", ", result.Reasons.Select(x => x.Message)));
                    }
                }

                await context.CommitAsync().ConfigureAwait(false);
                Logger.Information("Successfully imported {count} range events from {csvFileName}.", count,
                                   file);
                return ReturnCodes.SUCCESS;
            }
            catch (Exception ex)
            {
                await context.RollbackAsync().ConfigureAwait(false);
                Logger.Error(ex, "An error occurred while importing range events from {csvFileName}.", file);
                return ReturnCodes.RANGEEVENT_CSV_FILE_READ_FAILURE;
            }
        }

        /// <summary>
        ///     Loads SimpleRangeEvents from a CSV file.
        /// </summary>
        /// <param name="csvFileName">The path to the CSV file.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>An enumerable of SimpleRangeEvent objects.</returns>
        async IAsyncEnumerable<CsvRow> LoadRangeEventsFromCsv(string            csvFileName,
                                                              CancellationToken cancellationToken)
        {
            CsvConfiguration config = new(CultureInfo.InvariantCulture)
                                      {
                                          PrepareHeaderForMatch = args => args.Header.Replace("_", "").ToLower(),
                                      };

            using StreamReader reader = new(csvFileName);
            using CsvReader    csv    = new(reader, config);

            // GetRecordsAsync returns an IAsyncEnumerable<T>; ConfigureAwait is not applicable here.
            await foreach (CsvRow record in csv.GetRecordsAsync<CsvRow>(cancellationToken))
            {
                yield return record;
            }
        }

        record struct CsvRow(
            int    RowId,
            string Id,
            string EventDate,
            string FirearmName,
            string RangeName,
            int    RoundsFired,
            string AmmoDescription,
            string Notes,
            string Created,
            string Modified);
    }
}