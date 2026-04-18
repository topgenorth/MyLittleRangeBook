using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Database.Sqlite;
using Serilog;

namespace MyLittleRangeBook.Tests.Sqlite
{
    public class InMemorySqliteHelperTests
    {
        SqliteConnectionStringBuilder _builder;

        public InMemorySqliteHelperTests()
        {
            _builder = new SqliteConnectionStringBuilder { DataSource = ":memory:" };
        }

        [Fact]
        public async Task MigrationsTest()
        {
            ILogger logger = NSubstitute.Substitute.For<ILogger>();
            var helper = new SqliteHelper(logger, _builder.ConnectionString);

            await helper.ApplyDbupMigrationsAsync();


        }
    }
}
