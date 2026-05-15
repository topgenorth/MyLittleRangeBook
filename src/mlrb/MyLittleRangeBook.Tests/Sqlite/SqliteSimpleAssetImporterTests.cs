using System.Data;
using FluentResults;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Database.Sqlite;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Services;

namespace MyLittleRangeBook.Sqlite
{
    /// <summary>
    ///     Uses a tempory database connection for testing SqliteSimpleAssetImporter.
    /// </summary>
    public class SqliteSimpleAssetImporterTests : SqliteConnectionTestBase
    {
        async Task<MlrbId> InsertRangeEventRecordForTest()
        {
            const string SQL =
                """
                INSERT INTO SimpleRangeEvents (Id, EventDate, FirearmName, RangeName, RoundsFired, AmmoDescription, Notes, Created, Modified) 
                VALUES (@RangeEventId, '2014-09-11 00:00:00', 'Blunderbuss', 'Some range', 80, 'lead shot', 'TEST RECORD - DELETE', '2026-04-20 17:09:06.9231673+00:00', '2026-04-20 17:09:06.9280251+00:00');
                """;

            var mlrbId = new MlrbId();
            var cmd = new DapperCommand(SQL, new { RangeEventId = mlrbId.ToString() });
            await cmd.ExecuteAsync(await SqliteHelper.GetDatabaseConnectionAsync().ConfigureAwait(false))
                .ConfigureAwait(false);

            return mlrbId;
        }

        [Fact]
        public async Task Should_copy_file_and_link_to_rangevent()
        {
            await EnsureDatabaseExistsAsync();

            MlrbId rangeEventId = await InsertRangeEventRecordForTest();
            string pathToAsset = Path.GetTempFileName();
            IRangeEventAssetImporter inner = Substitute.For<IRangeEventAssetImporter>();
            inner.ImportAssetForRangeEvent(pathToAsset, rangeEventId).Returns((new MlrbId(), Path.GetTempFileName()));

            var sut = new SqliteSimpleAssetImporter(SqliteHelper, inner);
            Result<(MlrbId assetId, string destinationPath)> result =
                await sut.ImportAssetForRangeEvent(pathToAsset, rangeEventId);

            result.IsSuccess.ShouldBeTrue();

            result.Value.destinationPath.ShouldNotBeNullOrWhiteSpace();
            Path.Exists(result.Value.destinationPath).ShouldBeTrue();
            File.Delete(result.Value.destinationPath);

            result.Value.assetId.ToString().ShouldNotBeNullOrWhiteSpace();
            var cmd = new DapperCommand("SELECT COUNT(*) FROM SimpleRangeEvent_Images  WHERE ImageId=@ImageId;",
                new { ImageId = result.Value.assetId.ToString() });

            await using SqliteConnection  conn = await SqliteHelper.GetDatabaseConnectionAsync();
            long? r = await cmd.QuerySingleAsync<long?>(conn);
            r.ShouldNotBeNull();
            r.Value.ShouldBe(1);
        }
    }
}
