using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.CLI.Console;
using MyLittleRangeBook.Database.Sqlite;
using MyLittleRangeBook.IO;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Services;
using NanoidDotNet;

namespace MyLittleRangeBook.CLI.Database.Sqlite
{
    /// <summary>
    ///     Provides functionality to import FIT files into a SQLite database.
    /// </summary>
    [RegisterCommands("fit import")]
    public class ImportFitFileToSqliteCommand
    {
        readonly ILogger _logger;
        readonly ICliDisplay _cliDisplay;
        readonly IFitFilesDbService _filesDbService;
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
            IFitFilesDbService filesDbService,
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
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous import operation. The task result contains the exit code.</returns>
        [Command("sqlite")]
        [UsedImplicitly]
        public async Task<int> AddFitFileToDatabaseAsync(string fitFile,
            CancellationToken cancellationToken = default)
        {
            // TODO [TO20260419] Improve console output.

            _cliDisplay.WriteAppInfo("Importing FIT File");

            Result<ReadOnlyMemory<byte>> fileResult =
                await fitFile.LoadFileBytesAsync(cancellationToken).ConfigureAwait(false);
            if (fileResult.IsFailed)
            {
                _logger.Error("Failed to read FIT file {fileName}.", fitFile);
                _cliDisplay.WriteFailure($"Failed to read FIT file {fitFile}.");

                return ReturnCodes.FIT_FILE_READ_FAILURE;
            }

            await using SqliteConnection connection = await _sqliteHelper
                .GetDatabaseConnectionAsync(cancellationToken)
                .ConfigureAwait(false);
            string? id = await Nanoid.GenerateAsync().ConfigureAwait(false);
            Result<EntityId> fitResult = await _filesDbService
                .UpsertFitFileAsync(connection, id, fileResult.Value, fitFile, cancellationToken)
                .ConfigureAwait(false);

            if (fitResult.IsSuccess)
            {
                Success? success = new WroteFitFileToDatabaseSuccess(fitFile, fileResult.Value.Length)
                    .Enrich(fitResult.Value);
                _cliDisplay.WriteSuccess($"Saved {fileResult.Value.Length} bytes from FIT file {fitFile} to database.");

                return ReturnCodes.SUCCESS;
            }

            _logger.Warning("Failed to save the {fileName} to the database.", fitFile);
            _cliDisplay.WriteFailure($"Failed to save {fitFile} to the database.");
            return ReturnCodes.SQL_FAILED_TO_WRITE_TO_DATABASE;

        }
    }
}
