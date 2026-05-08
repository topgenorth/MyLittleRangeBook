using System.Data;
using FluentResults;
using MyLittleRangeBook.Database.Sqlite;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Services;

namespace MyLittleRangeBook.Sqlite
{
    public class SqliteSimpleRangeEventRepositoryTests : SqliteConnectionTestBase
    {
        [Fact]
        public async Task Should_Upsert_SimpleRangeEvent_With_FitFileContents()
        {
            await GetSqliteConnectionAsync();
            IFitFilesDbService fitFilesDbService = Substitute.For<IFitFilesDbService>();
            IShotViewFilesDbService shotViewFilesDbService = Substitute.For<IShotViewFilesDbService>();
            fitFilesDbService.UpsertFitFileAsync(Arg.Any<IDbConnection>(),
                    Arg.Any<string>(),
                    Arg.Any<ReadOnlyMemory<byte>>(),
                    Arg.Any<string?>())
                .Returns(Task.FromResult(Result.Ok().ToResult(new EntityId("x", 1)))
                );
            fitFilesDbService.AssociateWithRangeEvent(Arg.Any<IDbConnection>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(Task.FromResult(Result.Ok((long?)1)));


            var simpleRangeLogService = new SqliteSimpleRangeEventService();
            var repo = new SqliteSimpleRangeEventRepository(SqliteHelper, simpleRangeLogService, fitFilesDbService, shotViewFilesDbService);

            var simpleRangeEvent = SimpleRangeEvent.New("TestFirearm", 50, "TestRange", "TestAmmo", "TestNotes");
            byte[] fitFileContents = [1, 2, 3, 4, 5];

            Result<long?> result = await repo.UpsertAsync(simpleRangeEvent, fitFileContents);

            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value.Value.ShouldBeGreaterThan(0);
        }
    }
}
