using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.CLI.Console;
using MyLittleRangeBook.Database.Sqlite;
using MyLittleRangeBook.FIT;
using MyLittleRangeBook.IO;

namespace MyLittleRangeBook.CLI.Database.Sqlite
{
    /// <summary>
    ///     Provides functionality to import FIT files into a SQLite database.
    /// </summary>
    [RegisterCommands("fit import")]
    public class ImportFitFileToSqliteCommand
    {
        const string InsertFitFileSql =
            @"INSERT INTO fitfiles (id, filename, contents) VALUES (@id, @filename, @filecontents)";

        readonly ICliDisplay _cliDisplay;
        readonly ILogger _logger;
        readonly ISqliteHelper _sqliteHelper;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ImportFitFileToSqliteCommand" /> class.
        /// </summary>
        /// <param name="cliDisplay">The CLI display helper for user interaction.</param>
        /// <param name="logger">The logger for recording operation details.</param>
        /// <param name="sqliteHelper">The helper for SQLite database operations.</param>
        public ImportFitFileToSqliteCommand(ICliDisplay cliDisplay, ILogger logger, ISqliteHelper sqliteHelper)
        {
            _cliDisplay = cliDisplay;
            _logger = logger;
            _sqliteHelper = sqliteHelper;
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

            Result<int> result = await _cliDisplay.RunStatusAsync<Result<int>>("Importing FIT File",
                async ct => await DoWorkAsync(fitFile, ct), cancellationToken);

            if (result.IsSuccess)
            {
                _cliDisplay.WriteSuccess($"Imported FIT File {fitFile}.");
            }
            else
            {
                _cliDisplay.WriteFailure($"Failed to import FIT {fitFile}");
            }

            return result.IsSuccess ? result.Value : ReturnCodes.FIT_FILE_READ_FAILURE;
        }



        /// <summary>
        ///     Performs the actual work of importing the FIT sqliteFile.
        /// </summary>
        /// <param name="fitFile">The path to the FIT sqliteFile to be imported.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>
        ///     A task that represents the asynchronous work. The task result contains a <see cref="Result{T}" /> with the
        ///     operation status.
        /// </returns>
        async Task<Result<int>> DoWorkAsync(string fitFile, CancellationToken cancellationToken)
        {
            Result<ReadOnlyMemory<byte>> fileContents = await
                fitFile.LoadFileBytesAsync(cancellationToken).ConfigureAwait(false);
            if (fileContents.IsFailed)
            {
                _logger.Error("Failed to load FIT  {fitFile}.", fitFile);
                var err = new FailedToLoadFileError(fitFile);
                var r = Result.Fail<int>(err);

                return r;
            }

            byte[] bytesToSave = fileContents.Value.ToArray();

            await using SqliteConnection connection = await _sqliteHelper
                .GetDatabaseConnectionAsync(cancellationToken)
                .ConfigureAwait(false);
            Result<(string id, long rowId)> writeResult = await _sqliteHelper
                .WriteFileToTableAsync(connection, SqliteFileTable.FitFiles, fitFile, bytesToSave, cancellationToken)
                .ConfigureAwait(false);


            if (writeResult.IsSuccess)
            {
                Success? success = new WroteFitFileToDatabaseSuccess(fitFile, bytesToSave.Length)
                    .WithMetadata("RowId", writeResult.Value.rowId)
                    .WithMetadata("Id", writeResult.Value.id);
                _logger.Information("Saved {bytes} bytes from FIT sqliteFile {fitFile} to database.",
                    bytesToSave.Length, fitFile);

                return Result.Ok(ReturnCodes.SUCCESS).WithSuccess(success);
            }

            Error? err2 = new FailedToWriteFitFileToDatabaseError(fitFile, bytesToSave.Length);
            Result<int>? r2 = new Result<int>().WithValue(ReturnCodes.SQL_FAILED_TO_WRITE_TO_DATABASE).WithError(err2);

            return r2;
        }
    }
}
