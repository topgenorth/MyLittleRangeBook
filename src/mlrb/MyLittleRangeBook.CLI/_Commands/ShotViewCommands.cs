using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using MyLittleRangeBook.Database.Sqlite;
using MyLittleRangeBook.IO;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Services;
using NanoidDotNet;
using Spectre.Console;

namespace MyLittleRangeBook.CLI.Console
{
    [RegisterCommands("shotview")]
    [UsedImplicitly]
    public class ShotViewCommands
    {
        readonly ICliDisplay _cliDisplay;
        readonly ILogger _logger;
        readonly ISimpleRangeEventRepository _repo;
        readonly IShotViewFilesDbService _shotViewFilesDbService;
        readonly ISqliteHelper _sqliteHelper;

        public ShotViewCommands(ICliDisplay cliDisplay,
            [FromKeyedServices(SqliteHelperExtensions.DI_KEYS_SQLITE)] IShotViewFilesDbService shotViewFilesDbService,
            [FromKeyedServices(SqliteHelperExtensions.DI_KEYS_SQLITE)] ISimpleRangeEventRepository repo,
            ISqliteHelper sqliteHelper,
            ILogger logger)
        {
            _cliDisplay = cliDisplay;
            _shotViewFilesDbService = shotViewFilesDbService;
            _repo = repo;
            _sqliteHelper = sqliteHelper;
            _logger = logger;
        }

        /// <summary>
        ///     Add a ShotView CSV file to the database and optionally associate it with a range event.
        /// </summary>
        /// <param name="csvFile">The path to the ShotView CSV file. If omitted, you will be prompted.</param>
        /// <param name="rangeEventId">
        ///     The Nanoid for a given range event. If omitted, you will be prompted to optionally select
        ///     one.
        /// </param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Command("add")]
        [UsedImplicitly]
        public async Task<int> AddShotViewFileAsync(string csvFile = "",
            string? rangeEventId = null,
            CancellationToken cancellationToken = default)
        {
            _cliDisplay.PrintAppInfo();

            if (string.IsNullOrWhiteSpace(csvFile))
            {
                csvFile = await _cliDisplay.Console.PromptAsync(
                        new TextPrompt<string>("Enter the path to the ShotView [green]CSV file[/]:")
                            .Validate(path =>
                                File.Exists(path)
                                    ? ValidationResult.Success()
                                    : ValidationResult.Error("[red]File does not exist.[/]")),
                        cancellationToken)
                    .ConfigureAwait(false);
            }

            if (!File.Exists(csvFile))
            {
                _cliDisplay.PrintFailure($"File not found: {csvFile}");

                return ReturnCodes.SHOTVIEW_FILE_NOT_FOUND;
            }

            Result<string> fileResult = await csvFile.LoadFileTextAsync(cancellationToken).ConfigureAwait(false);
            if (fileResult.IsFailed)
            {
                IError? err = fileResult.Errors[0];
                string? msg = err.Message;
                _logger.Error(msg);
                _cliDisplay.PrintFailure(msg);

                return ReturnCodes.SHOTVIEW_FILE_READ_FAILURE;
            }

            await using SqliteConnection conn =
                await _sqliteHelper.GetDatabaseConnectionAsync(cancellationToken).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(rangeEventId))
            {
                string selection = await _cliDisplay.Console.PromptAsync(
                        new SelectionPrompt<string>()
                            .Title("Do you want to associate this file with a range event?")
                            .AddChoices("Yes", "No"),
                        cancellationToken)
                    .ConfigureAwait(false);

                if (selection == "Yes")
                {
                    Result<IEnumerable<SimpleRangeEvent>> eventsResult =
                        await _repo.GetSimpleRangeEventsAsync(cancellationToken).ConfigureAwait(false);
                    if (eventsResult.IsSuccess && eventsResult.Value.Any())
                    {
                        SimpleRangeEvent selectedEvent = await _cliDisplay.Console.PromptAsync(
                                new SelectionPrompt<SimpleRangeEvent>()
                                    .Title("Select a [green]range event[/] to associate with:")
                                    .PageSize(10)
                                    .MoreChoicesText("[grey](Move up and down to reveal more events)[/]")
                                    .AddChoices(eventsResult.Value)
                                    .UseConverter(e => $"{e.EventDate:yyyy-MM-dd} - {e.FirearmName} at {e.RangeName}"),
                                cancellationToken)
                            .ConfigureAwait(false);

                        rangeEventId = selectedEvent.Id;
                    }
                    else
                    {
                        _cliDisplay.PrintSuccess("No range events found to associate with.");
                    }
                }
            }

            string shotViewFileId = await Nanoid.GenerateAsync().ConfigureAwait(false);
            Result<EntityId> upsertResult = await _shotViewFilesDbService
                .UpsertShotViewFileAsync(conn, shotViewFileId, fileResult.Value, Path.GetFileName(csvFile),
                    cancellationToken)
                .ConfigureAwait(false);

            if (upsertResult.IsFailed)
            {
                _cliDisplay.PrintFailure("Failed to save ShotView file to database.");

                return ReturnCodes.SQL_FAILED_TO_WRITE_TO_DATABASE;
            }

            if (!string.IsNullOrWhiteSpace(rangeEventId))
            {
                Result<long?> associateResult = await _shotViewFilesDbService
                    .AssociateWithRangeEvent(conn, rangeEventId, upsertResult.Value.Id, cancellationToken)
                    .ConfigureAwait(false);
                if (associateResult.IsSuccess)
                {
                    _cliDisplay.PrintSuccess($"ShotView file associated with range event {rangeEventId}.");
                }
                else
                {
                    _cliDisplay.PrintFailure("Failed to associate ShotView file with range event.");
                }
            }

            _cliDisplay.PrintSuccess($"Successfully added ShotView file {csvFile} to database.");

            return ReturnCodes.SUCCESS;
        }
    }
}
