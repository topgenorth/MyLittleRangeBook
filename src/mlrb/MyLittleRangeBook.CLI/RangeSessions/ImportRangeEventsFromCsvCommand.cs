using System.Globalization;
using System.Runtime.CompilerServices;
using ConsoleAppFramework;
using CsvHelper;
using CsvHelper.Configuration;
using Fisher;
using JetBrains.Annotations;
using MyLittleRangeBook.Console;
using MyLittleRangeBook.RangeEvents;

namespace MyLittleRangeBook.RangeSessions
{
    [RegisterCommands("rangeevents")]
    [UsedImplicitly]
    public sealed class ImportRangeEventsFromCsvCommand
    {
        readonly ICliDisplay      _cliDisplay;
        readonly ILogger          _logger;
        readonly IDocumentSession _session;

        public ImportRangeEventsFromCsvCommand(ILogger logger, ICliDisplay display, IDocumentSession session)
        {
            _logger     = logger;
            _session    = session;
            _cliDisplay = display;
        }

        /// <summary>
        ///     Imports range session from a CSV file into the database. This will create a new range session.
        /// </summary>
        /// <param name="file">The file path to the CSV file containing range session.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>An integer indicating the result code of the operation.</returns>
        [Command("import-from-csv")]
        [UsedImplicitly]
        public async Task<int> ImportFromCsvFile(string file, CancellationToken cancellationToken = default)
        {
            _cliDisplay.PrintCommandHeader("Import range sessions from CSV");
            if (!File.Exists(file))
            {
                _cliDisplay.PrintFailure("The CSV file was not found.");
                _logger.Error("The CSV file '{csvFileName}' was not found.", file);
                return ReturnCodes.RANGE_EVENT_CSV_FILE_NOT_FOUND;
            }

            _cliDisplay.PrintInfo($"Processing CSV file {file}...");
            Guid causationId = Guid.CreateVersion7();
            try
            {
                int count = 0;
                await foreach (CsvRow csvRow in LoadRangeEventsFromCsv(file, cancellationToken).ConfigureAwait(false))
                {
                    Guid     id           = Guid.CreateVersion7();
                    Guid     correctionId = id;
                    DateOnly eventDate    = DateOnly.FromDateTime(DateTime.Parse(csvRow.EventDate));
                    SimpleRangeSessionCreatedFromCli e = new(id,
                                                             correctionId,
                                                             causationId,
                                                             eventDate,
                                                             csvRow.FirearmName.Trim(),
                                                             csvRow.RangeName.Trim(),
                                                             csvRow.RoundsFired,
                                                             csvRow.AmmoDescription.Trim(),
                                                             csvRow.Notes.Trim(),
                                                             DateTimeOffset.UtcNow);

                    var stream = _session.Events.StartStream<SimpleRangeSession>(id, e);

                    count++;
                }
                await _session.SaveChangesAsync(cancellationToken);
                _cliDisplay.PrintSuccess($"Successfully imported {count} range sessions from '{file}'.");
                _logger.Information("Successfully imported {count} range sessions from '{csvFileName}'.", count,
                                    file);
                return ReturnCodes.SUCCESS;
            }
            catch (Exception ex)
            {
                _cliDisplay.PrintFailure($"An error occurred while importing range sessions from '{file}'.");
                _logger.Error(ex, "An error occurred while importing range sessions from '{csvFileName}'.", file);
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