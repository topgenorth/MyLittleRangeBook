using System.Data;
using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using MyLittleRangeBook.CLI.Console;
using MyLittleRangeBook.Database;
using MyLittleRangeBook.FIT;
using NanoidDotNet;
using Npgsql;

namespace MyLittleRangeBook.CLI.Database.Postgres
{
    /// <summary>
    ///     Command for importing a Garmin FIT file into a PostgreSQL database.
    /// </summary>
    public class ImportFitFileToPostgresqlCommand
    {
        readonly ICliDisplay _cliDisplay;
        readonly IDatabaseHelper _databaseHelper;
        readonly ILogger _logger;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ImportFitFileToPostgresqlCommand" /> class.
        /// </summary>
        /// <param name="cliDisplay">The command-line display interface.</param>
        /// <param name="logger">The logger for recording operation progress and errors.</param>
        /// <param name="databaseHelper">The database helper for managing database connections.</param>
        public ImportFitFileToPostgresqlCommand(ICliDisplay cliDisplay, ILogger logger, IDatabaseHelper databaseHelper)
        {
            _cliDisplay = cliDisplay;
            _logger = logger;
            _databaseHelper = databaseHelper;
        }

        /// <summary>
        ///     Executes the import operation for a specified FIT file.
        /// </summary>
        /// <param name="sourceFile">The path to the source FIT file to be imported.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        ///     A task that represents the asynchronous import operation. The task result contains the status code (0 for
        ///     success).
        /// </returns>
        [Command("fit pgsql")]
        [UsedImplicitly]
        public async Task<int> ImportFileAsync(string sourceFile, CancellationToken cancellationToken = default)
        {
            _cliDisplay.WriteHeader("Importing FIT file to Postgresql");
            Result<int> result = await _cliDisplay.RunStatusAsync<Result<int>>("Importing FIT file to Postgresql",
                async ct => await DoWorkAsync(sourceFile, ct), cancellationToken);

            if (result.IsSuccess)
            {
                _cliDisplay.WriteSuccess("Imported FIT file to Postgresql");
            }
            else
            {
                _cliDisplay.WriteFailure("Failed to import FIT file to Postgresql");
            }

            return result.Value;
        }

        /// <summary>
        ///     Performs the actual work of reading the FIT file and saving its content to the database.
        /// </summary>
        /// <param name="sourceFile">The path to the source FIT file.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        ///     A task that represents the asynchronous work. The task result contains a <see cref="Result{T}" /> indicating
        ///     success or failure.
        /// </returns>
        async Task<Result<int>> DoWorkAsync(string sourceFile, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(sourceFile))
            {
                _logger.Error("FIT sqliteFile {fitFile} not found.", sourceFile);

                return Result.Fail(new FitFileNotFoundError(sourceFile)).ToResult(ReturnCodes.FIT_FILE_NOT_FOUND);
            }

            Result<ReadOnlyMemory<byte>> fileContents = await sourceFile.LoadFitFileBytesAsync(cancellationToken);
            if (fileContents.IsFailed)
            {
                _logger.Error("Failed to load FIT file {fitFile}.", sourceFile);

                return Result.Fail(new FailedToLoadFitFileError(sourceFile)).ToResult(ReturnCodes.FAILED_TO_LOAD);
            }

            byte[] bytesToSave = fileContents.Value.ToArray();


            int rowId;
            try
            {
                using IDbConnection connection = await _databaseHelper.GetDatabaseConnectionAsync(cancellationToken);
                rowId = await SaveBytesAsync((NpgsqlConnection)connection, bytesToSave, sourceFile, cancellationToken);
                Success? success = new WroteFitFileToDatabaseSuccess(sourceFile, bytesToSave.Length)
                    .WithMetadata("RowId", rowId);

                _logger.Information("Saved {bytes} bytes from FIT sqliteFile {fitFile} to database.",
                    bytesToSave.Length, sourceFile);

                return Result.Ok(ReturnCodes.SUCCESS).WithSuccess(success);
            }
            catch (Exception e)
            {
                rowId = -1;
                _logger.Error(e, "Failed to save {bytes} bytes from FIT fitFile {fitFile} to database.",
                    bytesToSave.Length, sourceFile);


                Error? err = new FailedToWriteFitFileToDatabaseError(sourceFile, bytesToSave.Length)
                    .CausedBy(e);

                return Result.Fail(err);
            }
        }

        /// <summary>
        ///     Saves the byte contents of a FIT file to the PostgreSQL database.
        /// </summary>
        /// <param name="connection">An open PostgreSQL connection.</param>
        /// <param name="fileContents">The byte array containing FIT file contents.</param>
        /// <param name="filename">The original filename of the FIT file.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        ///     A task that represents the asynchronous save operation. The task result contains the row ID of the inserted
        ///     record, or -1 if the operation failed.
        /// </returns>
        async Task<int> SaveBytesAsync(NpgsqlConnection connection,
            byte[] fileContents,
            string filename,
            CancellationToken cancellationToken = default)
        {
            var cmd = new NpgsqlCommand(
                "INSERT INTO FitFiles (Id, Filename, Contents) VALUES (@id, @filename, @filecontents) RETURNING RowId",
                connection);

            // TODO [TO20260408] Confirm the fields....
            cmd.Parameters.AddWithValue("@id", await Nanoid.GenerateAsync());
            cmd.Parameters.AddWithValue("@filename", filename);
            cmd.Parameters.AddWithValue("@filecontents", fileContents);

            object? rowId = await cmd.ExecuteScalarAsync(cancellationToken);

            return rowId is null ? -1 : (int)rowId;
        }
    }
}
