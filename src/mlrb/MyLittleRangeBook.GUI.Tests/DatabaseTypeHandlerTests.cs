using Dapper;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.GUI.Helper;
using MyLittleRangeBook.Models;
using NanoidDotNet;

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

            await connection.ExecuteAsync(
                """
                CREATE TABLE IF NOT EXISTS SimpleRangeEvents
                (
                    RowId           INTEGER PRIMARY KEY AUTOINCREMENT,
                    Id              TEXT                              NOT NULL, --NanoID unique key.
                    EventDate       TEXT                              NOT NULL, -- The date of the event.
                    FirearmName     TEXT                              NOT NULL, -- The name of the firearm. Should match the Firearms table
                    RangeName       TEXT, -- The name of the range.
                    RoundsFired     INTEGER DEFAULT 0                 NOT NULL, -- How many rounds were fired.
                    AmmoDescription TEXT,
                    Notes           TEXT,
                    Created  TEXT default CURRENT_TIMESTAMP          not null, -- The date the record was created.
                    Modified TEXT default CURRENT_TIMESTAMP          not null, -- The date the file was last modified.
                    CONSTRAINT SimpleRangeEvents_Id UNIQUE (ID)
                );
                """);


            var rangeEvent = new SimpleRangeEvent
            {
                Id = await Nanoid.GenerateAsync(),
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
                INSERT INTO SimpleRangeEvents (Id, EventDate, FirearmName, RangeName, RoundsFired, Created, Modified)
                VALUES (@Id, @EventDate, @FirearmName, @RangeName, @RoundsFired, @Created, @Modified)
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
