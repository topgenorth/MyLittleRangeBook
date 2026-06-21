using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Persistence.Sqlite;
using MyLittleRangeBook.Sqlite;
using Xunit;
using Dapper;

namespace MyLittleRangeBook.Tests.Sqlite
{
    public class ScopedSqliteConnectionTests : SqliteConnectionTestBase
    {
        [Fact]
        public async Task ScopedConnection_WithoutTransaction_DoesNotHaveTransaction()
        {
            await using var scoped = await SqliteHelper.GetScopedDatabaseConnectionAsync();
            
            Assert.Null(scoped.Transaction);
        }

        [Fact]
        public async Task ScopedConnection_WithTransaction_HasTransaction()
        {
            await using var scoped = await SqliteHelper.GetScopedDatabaseConnectionAsync(useTransaction: true);
            Assert.NotNull(scoped.Transaction);
        }

        [Fact]
        public async Task ScopedConnection_WithTransaction_CommitsOnDispose()
        {
            string tableName = "TestTable";
            await EnsureDatabaseExistsAsync();
            
            {
                await using var conn = await SqliteHelper.GetDatabaseConnectionAsync();
                await conn.ExecuteAsync($"CREATE TABLE {tableName} (Id INTEGER PRIMARY KEY, Val TEXT)");
            }

            {
                // This is what we want to test
                await using (var scoped = await SqliteHelper.GetScopedDatabaseConnectionAsync(useTransaction: true))
                {
                    await scoped.Connection.ExecuteAsync($"INSERT INTO {tableName} (Val) VALUES (@Val)", new { Val = "Test" }, scoped.Transaction);
                    // No explicit commit
                }
            }

            // Verify it was committed
            {
                await using var conn = await SqliteHelper.GetDatabaseConnectionAsync();
                var count = await conn.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM {tableName}");
                Assert.Equal(1, count);
            }
        }

        [Fact]
        public async Task ScopedConnection_WithTransaction_CanBeExplicitlyCommitted()
        {
            string tableName = "TestTableExplicit";
            await EnsureDatabaseExistsAsync();
            
            {
                await using var conn = await SqliteHelper.GetDatabaseConnectionAsync();
                await conn.ExecuteAsync($"CREATE TABLE {tableName} (Id INTEGER PRIMARY KEY, Val TEXT)");
            }

            {
                await using (var scoped = await SqliteHelper.GetScopedDatabaseConnectionAsync(useTransaction: true))
                {
                    await scoped.Connection.ExecuteAsync($"INSERT INTO {tableName} (Val) VALUES (@Val)", new { Val = "Test" }, scoped.Transaction);
                    await scoped.Transaction!.CommitAsync();
                    // Transaction is now committed, DisposeAsync should not fail or re-commit
                }
            }

            // Verify it was committed
            {
                await using var conn = await SqliteHelper.GetDatabaseConnectionAsync();
                var count = await conn.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM {tableName}");
                Assert.Equal(1, count);
            }
        }
    }
}
