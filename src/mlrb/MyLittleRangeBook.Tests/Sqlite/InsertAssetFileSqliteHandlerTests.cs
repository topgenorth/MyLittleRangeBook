using FluentResults;
using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.MlrbAssets;
using MyLittleRangeBook.MlrbAssets.Handlers;
using MyLittleRangeBook.Sqlite;

namespace MyLittleRangeBook.Tests.Sqlite
{
    public class InsertAssetFileSqliteHandlerTests : SqliteConnectionTestBase
    {
        [Fact]
        public async Task ExecuteAsync_Should_Handle_Sha256_Duplicates()
        {
            // Arrange
            await EnsureDatabaseExistsAsync();
            IFirearmsService?            firearmService = Substitute.For<IFirearmsService>();
            InsertAssetFileSqliteHandler handler        = new(SqliteHelper, firearmService);

            MlrbAssetAggregate agg1 = MlrbAssetAggregate.New("file1.txt", DateTimeOffset.UtcNow);
            agg1.FileFingerprinted("sha256-123", 100, DateTimeOffset.UtcNow);
            agg1.Copied("path1.txt", new byte[] { 1, 2, 3 }, DateTimeOffset.UtcNow);
            MlrbAssetFile file1 = new(agg1);

            PipelineContext<MlrbAssetFile> context1 = new() { Record = file1 };

            // Act 1: First insert
            Result result1 = await handler.ExecuteAsync(context1, ctx => Task.FromResult(Result.Ok()));

            // Assert 1
            result1.IsSuccess.ShouldBeTrue(result1.ToString());

            // Act 2: Second insert with same SHA256 but different ID
            MlrbAssetAggregate agg2 = MlrbAssetAggregate.New("file2.txt", DateTimeOffset.UtcNow);
            agg2.FileFingerprinted("sha256-123", 100, DateTimeOffset.UtcNow);
            agg2.Copied("path2.txt", new byte[] { 1, 2, 3 }, DateTimeOffset.UtcNow);
            MlrbAssetFile file2 = new(agg2);

            PipelineContext<MlrbAssetFile> context2 = new() { Record = file2 };

            Result result2 = await handler.ExecuteAsync(context2, ctx => Task.FromResult(Result.Ok()));

            // Assert 2
            // Currently this will likely fail because SHA256 is not passed to the DB properly
            // and the SQL doesn't handle conflicts yet.
            result2.IsSuccess.ShouldBeTrue("Duplicate SHA256 should be handled according to requirement");
        }
    }
}