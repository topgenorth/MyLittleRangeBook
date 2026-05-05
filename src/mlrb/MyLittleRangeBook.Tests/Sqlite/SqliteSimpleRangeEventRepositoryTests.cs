using FluentResults;
using MyLittleRangeBook.Database.Sqlite;
using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.Sqlite
{
    public class SqliteSimpleRangeEventRepositoryTests
    {

        [Fact]
        public async Task Should_Upsert_SimpleRangeEvent_With_FitFileContents()
        {

            ILogger? logger = Substitute.For<ILogger>();
            var helper = new SqliteHelper(logger, $"Data Source={Path.GetTempFileName()}");
            Result<bool> migrationResult = await helper.ApplyDbupMigrationsAsync();
            migrationResult.IsSuccess.ShouldBeTrue();

            var simpleRangeLogService = new SqliteSimpleRangeEventService();
            var repo = new SqliteSimpleRangeEventRepository(helper, simpleRangeLogService);

            var simpleRangeEvent = SimpleRangeEvent.New("TestFirearm", 50, "TestRange", "TestAmmo", "TestNotes");
            byte[] fitFileContents = [1, 2, 3, 4, 5];

            Result<long?> result = await repo.UpsertAsync(simpleRangeEvent, fitFileContents);

            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value.Value.ShouldBeGreaterThan(0);
        }
    }
}
