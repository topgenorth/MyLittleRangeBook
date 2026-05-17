using FluentResults;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Database.Sqlite;
using MyLittleRangeBook.Models;


namespace MyLittleRangeBook.Sqlite
{
    public class SqliteFitFilesDbServiceTests : SqliteConnectionTestBase
    {
        [Fact]
        public async Task Should_insert_fit_file()
        {
            await using SqliteConnection conn = await GetSqliteConnectionAsync();
            var sut = new SqliteFitFilesDbService();

            string id =new MlrbId().ToString();
            ReadOnlyMemory<byte> contents = new byte[] { 1, 2, 3, 4, 5 }; // Dummy FIT data

            Result<EntityId> result = await sut.UpsertFitFileAsync(conn, id, contents, null);

            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value.Id.ShouldBe(id);
            result.Value.RowId!.Value.ShouldBeGreaterThanOrEqualTo(1L);
        }

        [Fact]
        public async Task Should_get_fit_file()
        {
            await using SqliteConnection conn = await GetSqliteConnectionAsync();
            var sut = new SqliteFitFilesDbService();

            string id = new MlrbId().ToString();
            ReadOnlyMemory<byte> contents = new byte[] { 1, 2, 3, 4, 5 }; // Dummy FIT data

            // Insert first
            Result<EntityId> upsertResult = await sut.UpsertFitFileAsync(conn, id, contents, null);
            upsertResult.IsSuccess.ShouldBeTrue("Failed to upsert the FIT record in the database");

            // Now get
            Result<(EntityId EntityId, string FileName, ReadOnlyMemory<byte> contents)> getResult =
                await sut.GetFitFileAsync(conn, id);

            getResult.IsSuccess.ShouldBeTrue();
            getResult.Value.EntityId.Id.ShouldBe(id);
            getResult.Value.FileName.ShouldStartWith($"{id}-{DateTime.UtcNow:yyyyMMdd}");
            getResult.Value.contents.ToArray().ShouldBe(contents.ToArray());
        }

        [Fact]
        public async Task Should_update_fit_file()
        {
            await using SqliteConnection conn = await GetSqliteConnectionAsync();
            var sut = new SqliteFitFilesDbService();

            string id = new MlrbId().ToString();
            ReadOnlyMemory<byte> contents1 = new byte[] { 1, 2, 3, 4, 5 }; // Initial dummy FIT data
            var fileName1 = "test1.fit";

            // Insert first
            Result<EntityId> upsertResult1 = await sut.UpsertFitFileAsync(conn, id, contents1, fileName1);
            upsertResult1.IsSuccess.ShouldBeTrue();
            long originalRowId = upsertResult1.Value.RowId!.Value;

            // Update with new data
            ReadOnlyMemory<byte> contents2 = new byte[] { 6, 7, 8, 9, 10 }; // Updated dummy FIT data
            var fileName2 = "test2.fit";

            Result<EntityId> upsertResult2 = await sut.UpsertFitFileAsync(conn, id, contents2, fileName2);
            upsertResult2.IsSuccess.ShouldBeTrue();
            upsertResult2.Value.RowId!.Value.ShouldBe(originalRowId); // RowId should remain the same

            // Get and verify updated data
            Result<(EntityId EntityId, string FileName, ReadOnlyMemory<byte> contents)> getResult =
                await sut.GetFitFileAsync(conn, id);
            getResult.IsSuccess.ShouldBeTrue();
            getResult.Value.EntityId.Id.ShouldBe(id);
            getResult.Value.EntityId.RowId.ShouldBe(originalRowId);
            getResult.Value.FileName.ShouldBe(fileName2);
            getResult.Value.contents.ToArray().ShouldBe(contents2.ToArray());
        }

        [Fact]
        public async Task Should_delete_fit_file()
        {
            await using SqliteConnection conn = await GetSqliteConnectionAsync();
            var sut = new SqliteFitFilesDbService();

            string id =new MlrbId().ToString();
            ReadOnlyMemory<byte> contents = new byte[] { 1, 2, 3, 4, 5 }; // Dummy FIT data

            // Insert first
            Result<EntityId> upsertResult = await sut.UpsertFitFileAsync(conn, id, contents, null);
            upsertResult.IsSuccess.ShouldBeTrue();

            // Delete
            Result deleteResult = await sut.DeleteFitFileAsync(conn, id);
            deleteResult.IsSuccess.ShouldBeTrue();

            // Try to get, should fail
            Result<(EntityId EntityId, string FileName, ReadOnlyMemory<byte> contents)> getResult =
                await sut.GetFitFileAsync(conn, id);
            getResult.IsFailed.ShouldBeTrue();
        }
    }
}
