using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using MyLittleRangeBook.Database.Sqlite;
using MyLittleRangeBook.IO;
using MyLittleRangeBook.Services;

namespace MyLittleRangeBook.CLI
{
    /// <summary>
    ///     Save the FIT file to the SQLite database and optionally associate it with a range event.
    /// </summary>
    [RegisterCommands("fit import")]
    public class ImportFitFileToSqliteCommand: MlrbCommandBase
    {
        readonly IFitFilesDbService _filesDbService;
        readonly ISqliteHelper _sqliteHelper;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ImportFitFileToSqliteCommand" /> class.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cliDisplay">The CLI display helper for user interaction.</param>
        /// <param name="sqliteHelper">The helper for SQLite database operations.</param>
        /// <param name="filesDbService"></param>
        public ImportFitFileToSqliteCommand(ILogger logger,
            ICliDisplay cliDisplay,
            ISqliteHelper sqliteHelper,
            [FromKeyedServices(SqliteHelperExtensions.DI_KEYS_SQLITE)] IFitFilesDbService filesDbService): base(logger, cliDisplay)
        {
            _sqliteHelper = sqliteHelper;
            _filesDbService = filesDbService;
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

            CliDisplay.PrintCommandHeader("Importing FIT File");

            CliDisplay.PrintFailure("Currently being refactored.");
            return ReturnCodes.FAILURE;

            // TODO [TO20260419] Improve console output.
            CliDisplay.PrintCommandHeader("Importing FIT File");
            Logger.Information("Inserting FIT {fitFileName} into the database.", fitFile);

            Result<ReadOnlyMemory<byte>> fileResult = await fitFile
                .LoadFileBytesAsync(cancellationToken)
                .ConfigureAwait(false);
            if (fileResult.IsFailed)
            {
                IError? err = fileResult.Errors[0];
                string? msg = err.Message;
                Logger.Error(msg);
                CliDisplay.PrintFailure(msg);

                return ReturnCodes.FIT_FILE_READ_FAILURE;
            }

            await using SqliteConnection conn = await _sqliteHelper
                .GetDatabaseConnectionAsync(cancellationToken)
                .ConfigureAwait(false);

            string? fitFileId = new MlrbId().ToString();
            Result<EntityId> fitResult = await _filesDbService
                .UpsertFitFileAsync(conn, fitFileId, fileResult.Value, fitFile, cancellationToken)
                .ConfigureAwait(false);

            if (fitResult.IsSuccess)
            {
                if (!string.IsNullOrWhiteSpace(rangeEventId))
                {
                    Result<long?> associateResult = await _filesDbService
                        .AssociateWithRangeEvent(conn, rangeEventId, fitResult.Value.Id, cancellationToken)
                        .ConfigureAwait(false);
                    if (associateResult.IsSuccess)
                    {
                        Logger.Information("Associating FIT {fitFileName} with range event {rangeEventId}.",
                            fitFile,
                            rangeEventId);
                    }
                    else
                    {
                        Logger.Warning("Failed to associate FIT {fitFileName} with range event {rangeEventId}.",
                            fitFile,
                            rangeEventId);
                    }
                }

                CliDisplay.PrintSuccess($"Saved {fileResult.Value.Length} bytes from FIT file {fitFile} to database.");

                return ReturnCodes.SUCCESS;
            }

            return ReturnCodes.SQL_FAILED_TO_WRITE_TO_DATABASE;
        }
    }
}
