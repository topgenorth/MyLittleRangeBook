using System.Data;
using Dapper;
using FluentResults;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Services;

namespace MyLittleRangeBook.Database.Sqlite
{
    public class DuplicateShotViewFileNameError : MlrbBaseError
    {
        public DuplicateShotViewFileNameError(string fileName) : base(
            $"The file {fileName} already exists in the ShotView table")
        {
            FileName = fileName;
        }

        public string FileName { get; }
    }

    /// <summary>
    /// This Associate a ShotView file with an existing range event. The contents of the CSV
    ///  will be saved in the SQlite database.
    /// </summary>
    public class SqliteShotViewFilesDbService : IShotViewFilesDbService
    {
        const string SelectByIdSql = "SELECT * FROM ShotViewFiles WHERE Id=@Id;";

        const string InsertSql = """
                                 INSERT INTO ShotViewFiles (Id, FileName, Contents) 
                                 VALUES (@Id, @FileName, @Contents) 
                                 ON CONFLICT(id) DO UPDATE SET FileName = @FileName, Contents = @Contents, Modified=utcnow()
                                 RETURNING RowId;
                                 """;

        const string UpdateShotViewFileByName = """
                                                UPDATE ShotViewFiles
                                                SET Contents=@Contents, Modified=utcnow()  
                                                WHERE FileName=@FileName;
                                                """;

        const string DeleteSql = "DELETE FROM ShotViewFiles WHERE Id = @Id";

        const string AssociateWithRangeEventSql =
            "INSERT INTO SimpleRangeEvent_ShotViewFiles (SimpleRangeEventId, ShotViewFileId) VALUES (@RangeEventId, @ShotViewFileId) ON CONFLICT DO NOTHING RETURNING RowId";

        public async Task<Result<(EntityId EntityId, string FileName, string contents)>> GetShotViewFileAsync(
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

                ShotViewFileRow? record = await conn.QuerySingleOrDefaultAsync<ShotViewFileRow>(cd);

                if (record is null)
                {
                    Error err = new Error("ShotView file with ID not found in database").Enrich(id, null);

                    return Result.Fail(err);
                }

                var eid = new EntityId(record.Id, record.RowId);

                return Result.Ok((eid, record.FileName, record.Contents));
            }
            catch (Exception ex)
            {
                Error err = new Error("Unexpected exception trying to retrieve ShotView file from database.")
                    .CausedBy(ex)
                    .Enrich(id, null);

                return Result.Fail(err);
            }
        }

        public async Task<Result> DeleteShotViewFileAsync(IDbConnection connection,
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

                await conn.ExecuteAsync(cd);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                Error err = new Error("Unexpected exception trying to delete ShotView file.")
                    .CausedBy(ex)
                    .Enrich(id, null);

                return Result.Fail(err);
            }
        }

        public async Task<Result<EntityId>> UpsertShotViewFileAsync(IDbConnection connection,
            string id,
            string contents,
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
                fileName = $"{id}-{DateTime.UtcNow:yyyyMMddhhmm}.csv";
            }

            Result<EntityId> upsertResult =
                await SaveShotViewFileAsync(conn, id, fileName, contents, cancellationToken);
            if (upsertResult.IsSuccess)
            {
                return upsertResult;
            }

            Result<EntityId> updateResult =
                await SaveShotViewFileByFilenameAsync(conn, fileName, contents, cancellationToken);

            return updateResult;
        }

        public async Task<Result<long?>> AssociateWithRangeEvent(IDbConnection connection,
            string rangeEventId,
            string shotViewFileId,
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
                var p = new { RangeEventId = rangeEventId, ShotViewFileId = shotViewFileId };

                await conn.ExecuteScalarAsync(AssociateWithRangeEventSql, p);

                var cd = new CommandDefinition("SELECT RowId " +
                                               "FROM main.SimpleRangeEvent_ShotViewFiles " +
                                               "WHERE SimpleRangeEventId=@SimpleRangeEventId AND ShotViewFileId=@ShotViewFileId;",
                    new { SimpleRangeEventId = rangeEventId, ShotViewFileId = shotViewFileId },
                    cancellationToken: cancellationToken);
                long? l = await conn.ExecuteScalarAsync<long?>(cd);
                if (l is not null)
                {
                    return new Result<long?>().WithValue(Convert.ToInt64(l.Value));
                }

                var err = new Error("Could not find the association for the range event and ShotView file");

                return Result.Fail(err);
            }
            catch (Exception ex)
            {
                Error err = new Error("Unexpected exception trying to associate ShotView file with range event")
                    .CausedBy(ex);

                return Result.Fail(err);
            }
        }

        async Task<Result<EntityId>> SaveShotViewFileAsync(SqliteConnection conn,
            string id,
            string fileName,
            string contents,
            CancellationToken cancellationToken)
        {
            try
            {
                var cd = new CommandDefinition(InsertSql,
                    new { Id = id, FileName = fileName, Contents = contents },
                    cancellationToken: cancellationToken);

                long rowId = await conn.QuerySingleOrDefaultAsync<long>(cd);

                return new EntityId(id, rowId);
            }
            catch (SqliteException sex)
            {
                const int SQLITE_CONSTRAINT_VIOLATION = 19;
                const int SQLITE_CONSTRAINT_UNIQUE = 2067;

                Error err = new Error("Could not upsert ShotView file.").CausedBy(sex).Enrich(id, null);

                if (sex.SqliteErrorCode != SQLITE_CONSTRAINT_VIOLATION)
                {
                    return Result.Fail<EntityId>(err);
                }

                if (sex.SqliteExtendedErrorCode == SQLITE_CONSTRAINT_UNIQUE)
                {
                    err = new DuplicateShotViewFileNameError(fileName).CausedBy(sex).Enrich(id, null);
                }

                return Result.Fail<EntityId>(err);
            }
            catch (Exception ex)
            {
                Error err = new Error("Unexpected exception trying to upsert ShotView file")
                    .CausedBy(ex)
                    .Enrich(id, null);

                return Result.Fail<EntityId>(err);
            }
        }

        async Task<Result<EntityId>> SaveShotViewFileByFilenameAsync(SqliteConnection conn,
            string fileName,
            string contents,
            CancellationToken cancellationToken)
        {
            EntityId entityId;
            try
            {
                var getCd = new CommandDefinition(
                    "SELECT Id, RowId FROM main.ShotViewFiles WHERE FileName=@FileName",
                    new { FileName = fileName },
                    cancellationToken: cancellationToken);

                (string Id, long RowId) record = await conn.QuerySingleOrDefaultAsync<(string Id, long RowId)>(getCd);

                if (record == default)
                {
                    Error err = new Error("Could not find a ShotView record for the filename.")
                        .WithMetadata("FileName", fileName);

                    return Result.Fail(err);
                }

                entityId = new EntityId(record.Id, record.RowId);
            }
            catch (Exception ex)
            {
                Error err = new Error("Could not find a ShotView record for the filename.")
                    .CausedBy(ex)
                    .WithMetadata("FileName", fileName);

                return Result.Fail(err);
            }

            int rowsAffected;
            try
            {
                var updateCd = new CommandDefinition(UpdateShotViewFileByName,
                    new { FileName = fileName, Contents = contents },
                    cancellationToken: cancellationToken);
                rowsAffected = await conn.ExecuteAsync(updateCd);
            }
            catch (Exception ex)
            {
                rowsAffected = 0;
                Error err = new Error("Could not update the ShotView record for the filename.")
                    .CausedBy(ex)
                    .Enrich(entityId)
                    .WithMetadata("FileName", fileName);

                return Result.Fail(err);
            }

            if (rowsAffected == 0)
            {
                Error err = new Error("Did not update any ShotView records for the filename.")
                    .Enrich(entityId)
                    .WithMetadata("FileName", fileName);

                return Result.Fail(err);
            }

            return Result.Ok(entityId);
        }

        record ShotViewFileRow(
            long RowId,
            string Id,
            string FileName,
            string MimeType,
            string Contents,
            string Created,
            string Modified);
    }
}
