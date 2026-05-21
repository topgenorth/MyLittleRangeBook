using System.Text.Json.Nodes;
using FluentResults;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook.Sqlite
{
    public class SqliteHelperTests
    {
        const string InMemoryConnectionString = "Data Source=:memory:";

        const string AppSettingsWithConnectionString = """
                                                       {
                                                         "ConnectionStrings": {
                                                           "SqliteConnection": "Data Source=mlrb.db"
                                                         },
                                                         "Logging": {
                                                           "LogLevel": {
                                                             "Default": "Error",
                                                             "Microsoft.Hosting.Lifetime": "Error"
                                                           }
                                                         }
                                                       }
                                                       """;

        const string AppSettingsWithOutConnectionString = """
                                                          {
                                                            "Logging": {
                                                              "LogLevel": {
                                                                "Default": "Error",
                                                                "Microsoft.Hosting.Lifetime": "Error"
                                                              }
                                                            }
                                                          }
                                                          """;

        readonly ILogger _logger;

        public SqliteHelperTests()
        {
            _logger = Substitute.For<ILogger>();
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
            var helper = new SqliteHelper(_logger, InMemoryConnectionString);
            Result<bool> result = await helper.ApplyDbupMigrationsAsync();

            result.IsSuccess.ShouldBeTrue();
        }

        [Theory]
        [InlineData("{}", true)]
        [InlineData(AppSettingsWithConnectionString, false)]
        [InlineData(AppSettingsWithOutConnectionString, true)]
        public void SqliteExtensions_EnsureSqliteConnectionString(string? json, bool wasUpdated)
        {
            var n = JsonNode.Parse(json ?? "{}");
            n.ShouldNotBeNull();

            var expected = json switch
            {
                AppSettingsWithConnectionString => "Data Source=mlrb.db",
                _ => new SqliteConnectionStringBuilder
                {
                    DataSource = SqliteHelperExtensions.DefaultSqliteDatabaseName(),
                    Mode = SqliteOpenMode.ReadWriteCreate
                }.ConnectionString
            };

            n.EnsureDefaultSqliteConnectionString().ShouldBe(wasUpdated);
            n["ConnectionStrings"].ShouldNotBeNull();
            n["ConnectionStrings"]!["SqliteConnection"].ShouldNotBeNull();
            n["ConnectionStrings"]!["SqliteConnection"]!.GetValue<string>().ShouldBe(expected);
        }
    }
}
