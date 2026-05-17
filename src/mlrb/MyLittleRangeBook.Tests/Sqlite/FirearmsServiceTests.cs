using FluentResults;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Database.Sqlite;
using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.Sqlite
{
    public class FirearmsServiceTests : SqliteConnectionTestBase
    {
        [Fact]
        public async Task Show_update_row()
        {
            await using SqliteConnection conn = await GetSqliteConnectionAsync();
            var sut = new SqliteFirearmsDbService();
            var f0 = new Firearm {  Name = "Unit test", Notes = "Inserting" };

            //Insert
            Result<EntityId> result1 = await sut.UpsertAsync(conn, f0);
            result1.IsSuccess.ShouldBeTrue();
            result1.Value.Id.ShouldNotBeNullOrWhiteSpace();

            // Update
            var f1 = new Firearm { RowId = f0.RowId, Id = f0.Id, Name = "Unit test", Notes = "Updating" };
            Result<EntityId> result2 = await sut.UpsertAsync(conn, f1);
            result2.IsSuccess.ShouldBeTrue();
            result2.Value.Id.ShouldNotBeNullOrWhiteSpace();
            result2.Value.RowId!.Value.ShouldBeEquivalentTo(f0.RowId);
        }

        [Fact]
        public async Task Should_insert_row()
        {
            await using SqliteConnection conn = await GetSqliteConnectionAsync();
            var sut = new SqliteFirearmsDbService();

            var f = new Firearm { Name = "Unit test", Notes = "Inserting" };

            Result<EntityId> result = await sut.UpsertAsync(conn, f);
            result.IsSuccess.ShouldBeTrue();
            result.Value.Id.ShouldNotBeNullOrWhiteSpace();
            result.Value.RowId!.Value.ShouldBeGreaterThanOrEqualTo(1L);
        }
    }
}
