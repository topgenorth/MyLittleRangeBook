using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.CLI.Console;
using MyLittleRangeBook.Database.Sqlite;
using MyLittleRangeBook.FIT;
using NanoidDotNet;

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

            // TODO [TO20260419] IMprove console output.
            _cliDisplay.WriteHeader("Importing FIT File");

            Result<bool> migrations = await _sqliteHelper.ApplyDbupMigrationsAsync(cancellationToken);

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

            return result.IsSuccess ? result.Value : ReturnCodes.FAILED_TO_LOAD;
        }


        async Task<Result<ReadOnlyMemory<byte>>> LoadFitFileBytesAsync(string fitFile,
            CancellationToken cancellationToken)
        {
            if (!File.Exists(fitFile))
            {
                _logger.Error("FIT sqliteFile {fitFile} not found.", fitFile);
                var err = new FitFileNotFoundError(fitFile);
                var r = Result.Fail(err);

                return r;
            }

            Result<ReadOnlyMemory<byte>> fileContents = await fitFile.LoadFitFileBytesAsync(cancellationToken);

            return fileContents;
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
            Result<ReadOnlyMemory<byte>> fileContents = await LoadFitFileBytesAsync(fitFile, cancellationToken);
            if (fileContents.IsFailed)
            {
                _logger.Error("Failed to load FIT  {fitFile}.", fitFile);
                var err = new FailedToLoadFitFileError(fitFile);
                var r = Result.Fail(err);

                return r;
            }

            byte[] bytesToSave = fileContents.Value.ToArray();

            long rowId;
            Result<bool> migrationResult = await _sqliteHelper.ApplyDbupMigrationsAsync(cancellationToken);

            try
            {
                await using SqliteConnection connection =
                    await _sqliteHelper.GetDatabaseConnectionAsync(cancellationToken);
                rowId = await WriteBytesToDatabaseAsync(connection, bytesToSave, fitFile, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to save {bytes} bytes from FIT {fitFile} to database {database}.",
                    bytesToSave.Length, fitFile, fitFile);
                Error? err = new FailedToWriteFitFileToDatabaseError(fitFile, bytesToSave.Length).CausedBy(e);
                Result<int>? r = new Result<int>().WithValue(ReturnCodes.FAILED_TO_WRITE_TO_DATABASE).WithError(err);

                return r;
            }

            Success? success = new WroteFitFileToDatabaseSuccess(fitFile, bytesToSave.Length)
                .WithMetadata("RowId", rowId);

            _logger.Information("Saved {bytes} bytes from FIT sqliteFile {fitFile} to database.",
                bytesToSave.Length, fitFile);

            return Result.Ok(ReturnCodes.SUCCESS).WithSuccess(success);
        }

        /// <summary>
        ///     Saves the byte contents of a FIT file to the database.
        /// </summary>
        /// <param name="connection">An open SQLite connection.</param>
        /// <param name="fileContents">The byte array containing FIT file contents.</param>
        /// <param name="filename">The original filename of the FIT file.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>
        ///     A task that represents the asynchronous save operation. The task result contains the row ID of the inserted
        ///     record, or -1 if the operation failed.
        /// </returns>
        async Task<long> WriteBytesToDatabaseAsync(SqliteConnection connection,
            byte[] fileContents,
            string filename,
            CancellationToken cancellationToken = default)
        {
            var cmd = new SqliteCommand(InsertFitFileSql, connection);
            cmd.Parameters.AddWithValue("@id", await Nanoid.GenerateAsync());
            cmd.Parameters.AddWithValue("@filename", filename);
            cmd.Parameters.AddWithValue("@filecontents", fileContents);

            object? rowId = await cmd.ExecuteScalarAsync(cancellationToken);

            return rowId is null ? -1 : Convert.ToInt64(rowId);
        }
    }
}
