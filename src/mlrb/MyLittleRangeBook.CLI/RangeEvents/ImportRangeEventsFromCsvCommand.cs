using System.Globalization;
using System.Runtime.CompilerServices;
using ConsoleAppFramework;
using CsvHelper;
using CsvHelper.Configuration;
using FluentResults;
using JetBrains.Annotations;
using MyLittleRangeBook.Console;

namespace MyLittleRangeBook.RangeEvents
{
    [RegisterCommands("rangeevent")]
    [UsedImplicitly]
    public class ImportRangeEventsFromCsvCommand
    {
        readonly ILogger                  _logger;
        readonly ISimpleRangeEventService _service;

        public ImportRangeEventsFromCsvCommand(ILogger logger, ICliDisplay display, ISimpleRangeEventService service)
        {
            _logger  = logger;
            _service = service;
        }

        /// <summary>
        ///     Imports range events from a CSV file into the database. There are no "guard-rails" to prevent you from importing
        ///     the
        ///     same file twice. Doing so will duplicate things.
        /// </summary>
        /// <param name="file">The file path to the CSV file containing range events.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>An integer indicating the result code of the operation.</returns>
        [Command("import-from-csv")]
        [UsedImplicitly]
        public async Task<int> ImportFromCsvFile(string file, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(file))
            {
                _logger.Error("The CSV file '{csvFileName}' was not found.", file);
                return ReturnCodes.SHOTVIEW_FILE_NOT_FOUND;
            }

            try
            {
                int count = 0;
                await foreach (CsvRow csvRow in LoadRangeEventsFromCsv(file, cancellationToken).ConfigureAwait(false))
                {
                    DateOnly eventDate = DateOnly.FromDateTime(DateTime.Parse(csvRow.EventDate));
                    SimpleRangeEvent sre = SimpleRangeEvent.New(csvRow.FirearmName,
                                                                csvRow.RoundsFired,
                                                                csvRow.RangeName,
                                                                csvRow.AmmoDescription,
                                                                csvRow.Notes,
                                                                eventDate);

                    Result<Guid> rUpsert = await _service.UpsertAsync(sre, cancellationToken).ConfigureAwait(false);


                    if (rUpsert.IsSuccess)
                    {
                        count++;
                    }
                    else
                    {
                        _logger.Warning("Failed to import row {rowId}: {error}", csvRow.RowId,
                                        string.Join(", ", rUpsert.Reasons.Select(x => x.Message)));
                    }
                }

                _logger.Information("Successfully imported {count} range events from {csvFileName}.", count,
                                    file);
                return ReturnCodes.SUCCESS;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred while importing range events from {csvFileName}.", file);
                return ReturnCodes.RANGEEVENT_CSV_FILE_READ_FAILURE;
            }
        }

        /// <summary>
        ///     Loads SimpleRangeEvents from a CSV file.
        /// </summary>
        /// <param name="csvFileName">The path to the CSV file.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>An enumerable of SimpleRangeEvent objects.</returns>
        async IAsyncEnumerable<CsvRow> LoadRangeEventsFromCsv(string csvFileName,
                                                              [EnumeratorCancellation]
                                                              CancellationToken cancellationToken)
        {
            CsvConfiguration config = new(CultureInfo.InvariantCulture)
                                      {
                                          PrepareHeaderForMatch = args => args.Header.Replace("_", "").ToLower(),
                                      };

            using StreamReader reader = new(csvFileName);
            using CsvReader    csv    = new(reader, config);

            // GetRecordsAsync returns an IAsyncEnumerable<T>; ConfigureAwait is not applicable here.
            // ReSharper disable once UseConfigureAwaitFalse
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