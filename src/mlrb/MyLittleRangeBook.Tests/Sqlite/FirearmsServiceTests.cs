using FluentResults;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook.Sqlite
{
    public class FirearmsServiceTests : SqliteConnectionTestBase
    {
        [Fact]
        public async Task Show_update_row()
        {
            await using ScopedSqliteConnection conn = await GetSqliteConnectionAsync();
            var sut = new FirearmsService();

            Firearm f0 = Firearm.New("Unit test");
            f0.Notes = "Inserting";
            var ctx0 = new DapperCommandContext(conn);

            //Insert
            Result<EntityId> result1 = await sut.UpsertAsync(ctx0, f0);
            result1.IsSuccess.ShouldBeTrue();
            result1.Value.Id.ShouldNotBeNullOrWhiteSpace();

            // Update
            Firearm f1 = Firearm.New("Unit test");
            f1.Notes = "Updating";
            var ctx1 = new DapperCommandContext(conn);
            Result<EntityId> result2 = await sut.UpsertAsync(ctx1, f1);
            result2.IsSuccess.ShouldBeTrue();
            result2.Value.Id.ShouldNotBeNullOrWhiteSpace();
            result2.Value.RowId!.Value.ShouldBeEquivalentTo(f0.RowId);
        }

        [Fact]
        public async Task Should_insert_row()
        {
            await using ScopedSqliteConnection conn = await GetSqliteConnectionAsync();
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
