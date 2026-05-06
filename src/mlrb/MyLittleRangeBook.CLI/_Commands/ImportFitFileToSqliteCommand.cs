using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using MyLittleRangeBook.CLI.Console;
using MyLittleRangeBook.Database.Sqlite;
using MyLittleRangeBook.IO;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Services;
using NanoidDotNet;

namespace MyLittleRangeBook.CLI.Database.Sqlite
{
    /// <summary>
    ///     Save the FIT file to the SQLite database and optionally associate it with a range event.
    /// </summary>
    [RegisterCommands("fit import")]
    public class ImportFitFileToSqliteCommand
    {
        readonly ICliDisplay _cliDisplay;
        readonly IFitFilesDbService _filesDbService;
        readonly ILogger _logger;
        readonly ISqliteHelper _sqliteHelper;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ImportFitFileToSqliteCommand" /> class.
        /// </summary>
        /// <param name="cliDisplay">The CLI display helper for user interaction.</param>
        /// <param name="sqliteHelper">The helper for SQLite database operations.</param>
        /// <param name="filesDbService"></param>
        /// <param name="logger"></param>
        public ImportFitFileToSqliteCommand(ICliDisplay cliDisplay,
            ISqliteHelper sqliteHelper,
            [FromKeyedServices(SqliteHelperExtensions.DI_KEYS_SQLITE)] IFitFilesDbService filesDbService,
            ILogger logger)
        {
            _cliDisplay = cliDisplay;
            _sqliteHelper = sqliteHelper;
            _filesDbService = filesDbService;
            _logger = logger;
        }

        /// <summary>
        ///     Imports a FIT file into the specified SQLite database.
        /// </summary>
        /// <param name="fitFile">The path to the FIT sqliteFile to be imported.</param>
        /// <param name="rangeEventId">The Nanoid for a given range event.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous import operation. The task result contains the exit code.</returns>
        [Command("sqlite")]
        [UsedImplicitly]
        public async Task<int> AddFitFileToDatabaseAsync(string fitFile,
            string? rangeEventId = null,
            CancellationToken cancellationToken = default)
        {
            // TODO [TO20260419] Improve console output.
            _cliDisplay.WriteAppInfo("Importing FIT File");
            _logger.Information("Inserting FIT {fitFileName} into the database.", fitFile);

            Result<ReadOnlyMemory<byte>> fileResult = await fitFile
                .LoadFileBytesAsync(cancellationToken)
                .ConfigureAwait(false);
            if (fileResult.IsFailed)
            {
                IError? err = fileResult.Errors[0];
                string? msg = err.Message;
                _logger.Error(msg);
                _cliDisplay.WriteFailure(msg);

                return ReturnCodes.FIT_FILE_READ_FAILURE;
            }

            await using SqliteConnection conn = await _sqliteHelper
                .GetDatabaseConnectionAsync(cancellationToken)
                .ConfigureAwait(false);

            string? fitFileId = await Nanoid.GenerateAsync().ConfigureAwait(false);
            Result<EntityId> fitResult = await _filesDbService
                .UpsertFitFileAsync(conn, fitFileId, fileResult.Value, fitFile, cancellationToken)
                .ConfigureAwait(false);

            if (fitResult.IsSuccess)
            {
                if (!string.IsNullOrWhiteSpace(rangeEventId))
                {
                    Result<long?> associateResult = await _filesDbService
                        .AssociateWithRangeEvent(conn, rangeEventId, fitResult.Value.Id, cancellationToken);
                    if (associateResult.IsSuccess)
                    {
                        _logger.Information("Associating FIT {fitFileName} with range event {rangeEventId}.",
                            fitFile,
                            rangeEventId);
                    }
                    else
                    {
                        _logger.Warning("Failed to associate FIT {fitFileName} with range event {rangeEventId}.",
                            fitFile,
                            rangeEventId);
                    }
                }

                _cliDisplay.WriteSuccess($"Saved {fileResult.Value.Length} bytes from FIT file {fitFile} to database.");

                return ReturnCodes.SUCCESS;
            }

            return ReturnCodes.SQL_FAILED_TO_WRITE_TO_DATABASE;
        }
    }
}
