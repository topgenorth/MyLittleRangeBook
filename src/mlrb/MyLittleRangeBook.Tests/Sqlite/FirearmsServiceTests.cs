using FluentResults;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.Sqlite
{
    public class FirearmsServiceTests : SqliteConnectionTestBase
    {
        [Fact]
        public async Task Show_update_row()
        {
            await using SqliteConnection conn = await GetSqliteConnectionAsync();
            var sut = new FirearmsService();
            var f0 = new Firearm { Name = "Unit test", Notes = "Inserting" };

            //Insert
            Result<EntityId> result1 = await sut.UpsertAsync(f0, conn);
            result1.IsSuccess.ShouldBeTrue();
            result1.Value.Id.ShouldNotBeNullOrWhiteSpace();

            // Update
            var f1 = new Firearm { RowId = f0.RowId, Id = f0.Id, Name = "Unit test", Notes = "Updating" };
            Result<EntityId> result2 = await sut.UpsertAsync(f1, conn);
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

            Result<EntityId> result = await sut.UpsertAsync(f, conn);
            result.IsSuccess.ShouldBeTrue();
            result.Value.Id.ShouldNotBeNullOrWhiteSpace();
            result.Value.RowId!.Value.ShouldBeGreaterThanOrEqualTo(1L);
        }
    }
}
