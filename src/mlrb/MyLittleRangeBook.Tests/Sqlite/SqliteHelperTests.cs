using FluentResults;
using MyLittleRangeBook.Database.Sqlite;
using Serilog;
using Shouldly;
using SQLitePCL;

namespace MyLittleRangeBook.Tests.Sqlite
{
    public class SqliteHelperTests
    {
        const string InMemoryConnectionString = "Data Source=:memory:";
        readonly ILogger _logger;

        public SqliteHelperTests()
        {
            _logger = NSubstitute.Substitute.For<ILogger>();
        }

        [Fact]
        // ReSharper disable once AsyncMethodWithoutAwait
        public async Task Should_Append_ReadWriteCreate_To_ConnectionString()
        {
            var helper = new SqliteHelper(_logger, InMemoryConnectionString);

            var connectionString = helper.ToString();

            connectionString.ShouldNotBeNullOrWhiteSpace();
            connectionString.ShouldBe("Data Source=:memory:;Mode=ReadWriteCreate");
        }

        [Fact]
        public async Task Should_Apply_DbUp_Migrations()
        {
            // [TO20260419] Not a very good test - we just assume that the migrations will run if the result is success.
            SqliteHelper helper = new SqliteHelper(_logger, InMemoryConnectionString);
            Result<bool> result = await helper.ApplyDbupMigrationsAsync();

            result.IsSuccess.ShouldBeTrue();
        }
    }
}
