using System.Data;
using Dapper;
using FluentResults;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Services;

namespace MyLittleRangeBook.Database.Sqlite
{
    public class SqliteFitFilesDbService : IFitFilesDbService
    {
        const string SelectByIdSql = "SELECT * FROM FitFiles WHERE Id=@Id;";

        const string InsertSql = """
                                 INSERT INTO FitFiles (Id, FileName, Contents) 
                                 VALUES (@Id, @FileName, @Contents) 
                                 ON CONFLICT(id) DO UPDATE SET FileName = @FileName, Contents = @Contents, Modified=utcnow()
                                 RETURNING  ROWID;
                                 """;

        const string DeleteSql = "DELETE FROM FitFiles WHERE Id = @Id";

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

        public async Task<Result<EntityId>> UpsertFitFileAsync(IDbConnection connection,
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

            try
            {
                var cd = new CommandDefinition(InsertSql,
                    new { Id = id, FileName = fileName, Contents = contents.ToArray() },
                    cancellationToken: cancellationToken);

                long x = await conn.QuerySingleOrDefaultAsync<long>(cd);

                return new EntityId(id, x);
            }
            catch (Exception ex)
            {
                Error err = new Error("Unexpected exception trying to upsert FIT file")
                    .CausedBy(ex)
                    .Enrich(id, null);

                return Result.Fail<EntityId>(err);
            }
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
