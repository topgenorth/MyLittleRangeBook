using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using FluentResults;
using MyLittleRangeBook.Database.Sqlite;
using Serilog;
using Shouldly;

namespace MyLittleRangeBook.Sqlite
{
    public class SqliteHelperTests
    {
        const string InMemoryConnectionString = "Data Source=:memory:";
        readonly ILogger _logger;
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

        [Theory]
        [InlineData("{}", "Data Source=C:\\Users\\tom\\AppData\\Local\\MyLittleRangeBook\\mlrb-Development.db;Mode=ReadWriteCreate")]
        [InlineData(AppSettingsWithConnectionString, "Data Source=mlrb.db")]
        [InlineData(AppSettingsWithOutConnectionString, "Data Source=C:\\Users\\tom\\AppData\\Local\\MyLittleRangeBook\\mlrb-Development.db;Mode=ReadWriteCreate")]
        public void SqliteExtensions_EnsureSqliteConnectionString(string? json, string expected)
        {
            var n = JsonNode.Parse(json ??"{}");
            n.ShouldNotBeNull();

            n.EnsureDefaultSqliteConnectionString().ShouldBeTrue();
            n["ConnectionStrings"].ShouldNotBeNull();
            n["ConnectionStrings"]!["SqliteConnection"].ShouldNotBeNull();
            n["ConnectionStrings"]!["SqliteConnection"]!.GetValue<string>().ShouldBe(expected);
        }
    }
}
