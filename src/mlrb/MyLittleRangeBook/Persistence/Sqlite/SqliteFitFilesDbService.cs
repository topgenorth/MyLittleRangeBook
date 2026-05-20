using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.RangeEventAssets;

namespace MyLittleRangeBook.Persistence.Sqlite
{
    [Obsolete("Don't use", true)]
    public class SqliteFitFilesDbService
    {
        const string SelectByIdSql = "SELECT * FROM FitFiles WHERE Id=@Id;";

        /// <summary>
        ///     This SQL statement inserts a new record into the FitFiles table with specified values for Id, FileName, and
        ///     Contents.
        ///     If a record with the same Id already exists, it updates the existing record with the new FileName, Contents,
        ///     and sets the Modified timestamp to the current UTC time. The statement returns the RowId of the affected row.
        /// </summary>
        const string InsertSql = """
                                 INSERT INTO FitFiles (Id, FileName, Contents) 
                                 VALUES (@Id, @FileName, @Contents) 
                                 ON CONFLICT(id) DO UPDATE SET FileName = @FileName, Contents = @Contents, Modified=utcnow()
                                 RETURNING RowId;
                                 """;

        /// <summary>
        ///     This SQL will update the contents of a FitFile record by its FileName, setting the Modified timestamp to the
        ///     current UTC time.
        /// </summary>
        const string UpdateFitFileByName = """
                                           UPDATE FitFiles
                                           SET Contents=@Contents, Modified=utcnow()  
                                           WHERE FileName=@FileName;
                                           """;

        /// <summary>
        ///     This SQL command deletes a FitFile record from the database where the ID matches the provided parameter.
        /// </summary>
        const string DeleteSql = "DELETE FROM FitFiles WHERE Id = @Id";

        /// <summary>
        ///     This SQL inserts an association between a range event and a fit file into the
        ///     SimpleRangeEvent_FitFiles table. If the association already exists, no action is taken.
        ///     The query returns the RowId of the inserted or existing record.
        /// </summary>
        const string AssociateWithRangeEventSql =
            "INSERT INTO SimpleRangeEvent_FitFiles (SimpleRangeEventId, FitFileId) VALUES (@RangeEventId, @FitFileId) ON CONFLICT DO NOTHING RETURNING RowId";

        public async Task<Result<(EntityId EntityId, string FileName, ReadOnlyMemory<byte> contents)>> GetFitFileAsync(
            IDbConnection connection,
            string id,
            CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Result.Fail("Operation cancelled by user");
            }

            if (connection is not SqliteConnection conn)
            {
                return Result.Fail("Connection is not a SqliteConnection");
            }

            try
            {
                var cd = new CommandDefinition(SelectByIdSql,
                    new { Id = id },
                    cancellationToken: cancellationToken);

                FitFileRow? record = await conn.QuerySingleOrDefaultAsync<FitFileRow>(cd);

                if (record is null)
                {
                    Error err = new Error("FIT file with ID not found in database").Enrich(id, null);

                    return Result.Fail(err);
                }

                var eid = new EntityId(record.Id, record.RowId);
                (EntityId eid, string fileName, ReadOnlyMemory<byte> contents) x = (eid,
                    fileName: record.FileName,
                    contents: new ReadOnlyMemory<byte>(record.Contents));

                return Result.Ok(x);
            }
            catch (Exception ex)
            {
                Error err = new Error("Unexpected exception trying to retrieve FIT file from database.")
                    .CausedBy(ex)
                    .Enrich(id, null);

                return Result.Fail(err);
            }
        }

        public async Task<Result> DeleteFitFileAsync(IDbConnection connection,
            string id,
            CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Result.Fail("Operation cancelled by user");
            }

            if (connection is not SqliteConnection conn)
            {
                return Result.Fail("Connection is not a SqliteConnection");
            }

            try
            {
                var cd = new CommandDefinition(DeleteSql,
                    new { Id = id },
                    cancellationToken: cancellationToken);

                long x = await conn.QuerySingleOrDefaultAsync<long>(cd);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                Error err = new Error("Unexpected exception trying to delete FIT file.")
                    .CausedBy(ex)
                    .Enrich(id, null);

                return Result.Fail(err);
            }
        }

        public async Task<Result<EntityId>> UpsertFitFdileAsync(IDbConnection connection,
            string id,
            ReadOnlyMemory<byte> contents,
            string? fileName,
            CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Result.Fail("Operation cancelled by user");
            }

            if (connection is not SqliteConnection conn)
            {
                return Result.Fail("Connection is not a SqliteConnection");
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = $"{id}-{DateTime.UtcNow:yyyyMMddhhmm}.fit";
            }

            Result<EntityId> upsertResult = await SaveFitFileAsync(conn, id, fileName, contents, cancellationToken);
            if (upsertResult.IsSuccess)
            {
                return upsertResult;
            }

            Result<EntityId> updateResult =
                await SaveFitFileByFilenameAsync(conn, fileName, contents, cancellationToken);

            return updateResult;
        }

        public async Task<Result<long?>> AssociateWithRangeEvent(IDbConnection connection,
            string rangeEventId,
            string fitFileId,
            CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Result.Fail("Operation cancelled by user");
            }

            if (connection is not SqliteConnection conn)
            {
                return Result.Fail("Connection is not a SqliteConnection");
            }

            try
            {
                var p = new { RangeEventId = rangeEventId, FitFileId = fitFileId };

                await conn.ExecuteScalarAsync(AssociateWithRangeEventSql, p);

                // [TO20260506] Need to retrieve the RowId of the association.
                var cd = new CommandDefinition("SELECT RowId " +
                                               "FROM main.SimpleRangeEvent_FitFiles " +
                                               "WHERE SimpleRangeEventId=@SimpleRangeEventId AND FitFileId=@FitFileId;",
                    new { SimpleRangeEventId = rangeEventId, FitFileId = fitFileId },
                    cancellationToken: cancellationToken);
                long? l = await conn.ExecuteScalarAsync<long?>(cd);
                if (l is not null)
                {
                    return new Result<long?>().WithValue(Convert.ToInt64(l.Value));
                }

                var err = new Error("Could not find the association for the range event and FIT file");

                return Result.Fail(err);
            }
            catch (Exception ex)
            {
                Error err = new Error("Unexpected exception trying to upsert FIT file")
                    .CausedBy(ex);

                return Result.Fail(err);
            }
        }

        async Task<Result<EntityId>> SaveFitFileAsync(SqliteConnection conn,
            string id,
            string fileName,
            ReadOnlyMemory<byte> contents,
            CancellationToken cancellationToken)
        {
            try
            {
                var cd = new CommandDefinition(InsertSql,
                    new { Id = id, FileName = fileName, Contents = contents.ToArray() },
                    cancellationToken: cancellationToken);

                long rowId = await conn.QuerySingleOrDefaultAsync<long>(cd);

                return new EntityId(id, rowId);
            }
            catch (SqliteException sex)
            {
                // [TO20260506] Detect if we've trigger a unique constraint violation.
                // If we have, it's probably a duplicate file name.
                const int SQLITE_CONSTRAINT_VIOLATION = 19;
                const int SQLITE_CONSTRAINT_UNIQUE = 2067;

                Error err = new Error("Could not upsert FIT file.").CausedBy(sex).Enrich(id, null);
                // SQLite Error 19: 'UNIQUE constraint failed: FitFiles.FileName'.

                if (sex.SqliteErrorCode != SQLITE_CONSTRAINT_VIOLATION)
                {
                    return Result.Fail<EntityId>(err);
                }

                if (sex.SqliteExtendedErrorCode == SQLITE_CONSTRAINT_UNIQUE)
                {
                    err = new DuplicateFitFileNameError(fileName).CausedBy(sex).Enrich(id, null);
                }

                return Result.Fail<EntityId>(err);
            }
            catch (Exception ex)
            {
                Error err = new Error("Unexpected exception trying to upsert FIT file")
                    .CausedBy(ex)
                    .Enrich(id, null);

                return Result.Fail<EntityId>(err);
            }
        }

        /// <summary>
        ///     Update an existing FIT record based on the filename.
        /// </summary>
        /// <remarks>
        ///     This assumes that the FIT record will exist.
        /// </remarks>
        /// <param name="conn"></param>
        /// <param name="fileName"></param>
        /// <param name="contents"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>An <c cref="Result{EntityId}" /> holding the RowId and the ID of the record.</returns>
        async Task<Result<EntityId>> SaveFitFileByFilenameAsync(SqliteConnection conn,
            string fileName,
            ReadOnlyMemory<byte> contents,
            CancellationToken cancellationToken)
        {
            EntityId entityId;
            try
            {
                // [TO20260506] Get the RowId and the ID for the record.
                var getCd = new CommandDefinition(
                    "SELECT Id, RowId FROM main.FitFiles WHERE FileName=@FileName",
                    new { FileName = fileName },
                    cancellationToken: cancellationToken);

                (string Id, long RowId) record = await conn.QuerySingleOrDefaultAsync<(string Id, long RowId)>(getCd);

                if (record == default)
                {
                    Error err = new Error("Could not find a FIT record for the filename.")
                        .WithMetadata("FileName", fileName);

                    return Result.Fail(err);
                }

                entityId = new EntityId(record.Id, record.RowId);
            }
            catch (Exception ex)
            {
                Error err = new Error("Could not find a FIT record for the filename.")
                    .CausedBy(ex)
                    .WithMetadata("FileName", fileName);

                return Result.Fail(err);
            }

            int rowsAffected;
            try
            {
                // [TO20260506] Update based on the filename.
                var updateCd = new CommandDefinition(UpdateFitFileByName,
                    new { FileName = fileName, Contents = contents.ToArray() },
                    cancellationToken: cancellationToken);
                rowsAffected = await conn.ExecuteAsync(updateCd);
            }
            catch (Exception ex)
            {
                rowsAffected = 0;
                Error err = new Error("Could not update the FIT record for the filename.")
                    .CausedBy(ex)
                    .Enrich(entityId)
                    .WithMetadata("FileName", fileName);

                return Result.Fail(err);
            }

            if (rowsAffected == 0)
            {
                Error err = new Error("Did not update any FIT records for the filename.")
                    .Enrich(entityId)
                    .WithMetadata("FileName", fileName);

                return Result.Fail(err);
            }

            // [TO20260506] If we make it this far, all good
            return Result.Ok(entityId);
        }

        record FitFileRow(
            long RowId,
            string Id,
            string FileName,
            string MimeType,
            byte[] Contents,
            string Created,
            string Modified);
    }
}
