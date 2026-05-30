using FluentResults;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook.Sqlite
{
    public class FirearmsServiceTests : SqliteConnectionTestBase
    {
        [Fact]
        public async Task Show_update_row()
        {
            await using SqliteConnection conn = await GetSqliteConnectionAsync();
            var sut = new FirearmsService();

            var f0 = new Firearm("Unit test") { Notes = "Inserting" };
            var ctx0 = new DapperCommandContext(conn);

            //Insert
            Result<EntityId> result1 = await sut.UpsertAsync(ctx0, f0);
            result1.IsSuccess.ShouldBeTrue();
            result1.Value.Id.ShouldNotBeNullOrWhiteSpace();

            // Update
            var f1 = new Firearm("Unit test") { RowId = f0.RowId, Id = f0.Id, Notes = "Updating" };
            var ctx1 = new DapperCommandContext(conn);
            Result<EntityId> result2 = await sut.UpsertAsync(ctx1, f1);
            result2.IsSuccess.ShouldBeTrue();
            result2.Value.Id.ShouldNotBeNullOrWhiteSpace();
            result2.Value.RowId!.Value.ShouldBeEquivalentTo(f0.RowId);
        }

        [Fact]
        public async Task Should_insert_row()
        {
            await using SqliteConnection conn = await GetSqliteConnectionAsync();
            var sut = new FirearmsService();

            var f = new Firearm { Name = "Unit test", Notes = "Inserting" };

            var ctx = new DapperCommandContext(conn);
            Result<EntityId> result = await sut.UpsertAsync(ctx, f);
            result.IsSuccess.ShouldBeTrue();
            result.Value.Id.ShouldNotBeNullOrWhiteSpace();
            result.Value.RowId!.Value.ShouldBeGreaterThanOrEqualTo(1L);
        }
    }
}
