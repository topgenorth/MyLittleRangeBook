using Dapper;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Gui.Helper;
using MyLittleRangeBook.Gui.Models;

namespace MyLittleRangeBook.GUI.Tests
{
    public class DatabaseTypeHandlerTests : TestBase
    {
        [Fact]
        public async Task Should_Query_DateTimeOffset_From_Sqlite()
        {
            // Explicitly register handlers as they are registered in Program.Main which might not run for tests
            SqlMapper.AddTypeHandler(typeof(DateTimeOffset), new SQLiteDateTimeOffsetHandler());
            SqlMapper.AddTypeHandler(typeof(DateTimeOffset?), new SQLiteDateTimeOffsetHandler());

            await using var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();


            var rangeEvent = new SimpleRangeEvent
            {
                EventDate = DateTime.Now,
                FirearmName = "Test Firearm",
                RangeName = "Test Range",
                RoundsFired = 50,
                Created = DateTimeOffset.Now,
                Modified = DateTimeOffset.Now
            };

            // Save using Dapper
            await connection.ExecuteAsync(
                """
                INSERT INTO SimpleRangeEvents (EventDate, FirearmName, RangeName, RoundsFired, Created, Modified)
                VALUES (@EventDate, @FirearmName, @RangeName, @RoundsFired, @Created, @Modified)
                """, rangeEvent);

            // Query back - this is where it fails in the issue description
            var events =
                (await connection.QueryAsync<SimpleRangeEvent>(
                    "SELECT * FROM SimpleRangeEvents WHERE FirearmName = 'Test Firearm'")).ToList();

            Assert.Single(events);
            Assert.Equal(rangeEvent.FirearmName, events[0].FirearmName);
            Assert.Equal(rangeEvent.Created.ToString("O"), events[0].Created.ToString("O"));
        }
    }
}
