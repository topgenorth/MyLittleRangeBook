using Dapper;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.RangeEvents;

namespace MyLittleRangeBook.Sqlite
{
    public class SqliteDateTimeOffsetHandlerTests : SqliteConnectionTestBase
    {
        [Fact]
        public async Task Should_Query_DateTimeOffset_From_Sqlite()
        {
            // Explicitly register handlers as they are registered in Program.Main which might not run for tests

            await using SqliteConnection connection = await GetSqliteConnectionAsync();

            var rangeEvent = new SimpleRangeEvent(DateTime.Now)
            {
                FirearmName = "Test Firearm",
                RangeName = "Test Range",
                RoundsFired = 50,
                Created = DateTimeOffset.Now,
                Modified = DateTimeOffset.Now
            };

            // Save using Dapper
            await connection.ExecuteAsync(
                """
                INSERT INTO simple_range_events (id, event_date, firearm_name, range_name, rounds_fired, created, modified)
                VALUES (@Id, @EventDate, @FirearmName, @RangeName, @RoundsFired, @Created, @Modified)
                """, rangeEvent);

            // Query back - this is where it fails in the issue description
            var events =
                (await connection.QueryAsync<SimpleRangeEvent>(
                    "SELECT id AS Id, event_date AS EventDate, firearm_name AS FirearmName, created AS Created FROM simple_range_events WHERE firearm_name = 'Test Firearm'")).ToList();

            Assert.Single(events);
            Assert.Equal(rangeEvent.FirearmName, events[0].FirearmName);
            Assert.Equal(rangeEvent.Created.ToString("O"), events[0].Created.ToString("O"));
        }
    }
}
