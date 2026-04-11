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
    [RegisterCommands("import fit")]
    public class ImportFitFileToSqliteCommand
    {
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
        /// <param name="sqliteFile">The path to the SQLite database sqliteFile.</param>
        /// <param name="fitFile">The path to the FIT sqliteFile to be imported.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous import operation. The task result contains the exit code.</returns>
        [Command("sqlite")]
        [UsedImplicitly]
        public async Task<int> AddFitFileToDatabaseAsync(string sqliteFile,
            string fitFile,
            CancellationToken cancellationToken = default)
        {
            _cliDisplay.WriteHeader("Importing FIT File");

            Result<int> result = await _cliDisplay.RunStatusAsync<Result<int>>("Importing FIT File",
                async ct => await DoWorkAsync(sqliteFile, fitFile, ct), cancellationToken);

            if (result.IsSuccess)
            {
                _cliDisplay.WriteFailure("Failed to import FIT sqliteFile");
            }
            else
            {
                _cliDisplay.WriteSuccess("Imported FIT File");
            }

            return result.Value;
        }

        /// <summary>
        ///     Performs the actual work of importing the FIT sqliteFile.
        /// </summary>
        /// <param name="sqliteFile">The path to the SQLite database sqliteFile.</param>
        /// <param name="fitFile">The path to the FIT sqliteFile to be imported.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>
        ///     A task that represents the asynchronous work. The task result contains a <see cref="Result{T}" /> with the
        ///     operation status.
        /// </returns>
        async Task<Result<int>> DoWorkAsync(string sqliteFile, string fitFile, CancellationToken cancellationToken)
        {
            if (!File.Exists(sqliteFile))
            {
                _logger.Error("Database sqliteFile {sqliteFile} not found.", sqliteFile);

                return Result.Fail(new SqliteDatabaseNotFoundError(sqliteFile))
                    .ToResult(ReturnCodes.DATABASE_FILE_NOT_FOUND);
            }

            if (!File.Exists(fitFile))
            {
                _logger.Error("FIT sqliteFile {fitFile} not found.", fitFile);

                return Result.Fail(new FitFileNotFoundError(sqliteFile)).ToResult(ReturnCodes.FIT_FILE_NOT_FOUND);
            }


            Result<ReadOnlyMemory<byte>> fileContents = await fitFile.LoadFitFileBytesAsync(cancellationToken);
            if (fileContents.IsFailed)
            {
                _logger.Error("Failed to load FIT sqliteFile {sqliteFile}.", sqliteFile);

                return Result.Fail(new FailedToLoadFitFileError(sqliteFile)).ToResult(ReturnCodes.FAILED_TO_LOAD);
            }

            byte[] bytesToSave = fileContents.Value.ToArray();

            int rowId = -1;
            try
            {
                await using SqliteConnection connection = await _sqliteHelper.GetDatabaseConnectionAsync(cancellationToken) ;
                rowId = await SaveBytesAsync(connection, bytesToSave, fitFile, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to save {bytes} bytes from FIT fitFile {fitFile} to database {database}.",
                    bytesToSave.Length, fitFile, sqliteFile);

                Error? err = new Error($"Failed to write to database. {e.Message}")
                    .WithMetadata("Filename", fitFile)
                    .CausedBy(e);

                return Result.Fail(err).ToResult(ReturnCodes.FAILED_TO_WRITE_TO_DATABASE);
            }

            Success? success = new WroteFitFileToDatabaseSuccess(fitFile, bytesToSave.Length)
                .WithMetadata("RowId", rowId);

            _logger.Information("Saved {bytes} bytes from FIT sqliteFile {fitFile} to database {database}.",
                bytesToSave.Length, fitFile, sqliteFile);

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
        async Task<int> SaveBytesAsync(SqliteConnection connection,
            byte[] fileContents,
            string filename,
            CancellationToken cancellationToken = default)
        {
            var cmd = new SqliteCommand(
                "INSERT INTO FitFiles (Id, Filename, Contents) VALUES (@id, @filename, @filecontents) RETURNING RowId",
                connection);
            cmd.Parameters.AddWithValue("@id", await Nanoid.GenerateAsync());
            cmd.Parameters.AddWithValue("@filename", filename);
            cmd.Parameters.AddWithValue("@filecontents", fileContents.Length == 0 ? fileContents : []);

            object? rowId = await cmd.ExecuteScalarAsync(cancellationToken);

            return rowId is null ? -1 : (int)rowId;
        }
    }
}
