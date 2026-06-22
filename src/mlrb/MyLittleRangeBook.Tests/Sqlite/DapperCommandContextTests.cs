using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;
using Shouldly;
using Dapper;
using Xunit;

namespace MyLittleRangeBook.Sqlite
{
    public class DapperCommandContextTests : SqliteConnectionTestBase
    {
        [Fact]
        public async Task Dispose_ShouldCommitByDefault()
        {
            // Arrange
            var tableName = "TestTable_Commit";
            await using (var setupConn = await SqliteHelper.GetScopedDatabaseConnectionAsync())
            {
                await setupConn.Connection.ExecuteAsync($"CREATE TABLE {tableName} (Id INTEGER PRIMARY KEY, Val TEXT)");
            }

            // Act
            {
                await using var ctx = await DapperCommandContext.NewAsync(SqliteHelper, withTransaction: true);
                await ctx.Connection.ExecuteAsync($"INSERT INTO {tableName} (Val) VALUES ('Test')", transaction: ctx.Transaction);
                // DisposeAsync (via using) should commit
            }

            // Assert
            await using (var assertConn = await SqliteHelper.GetScopedDatabaseConnectionAsync())
            {
                var count = await assertConn.Connection.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM {tableName}");
                count.ShouldBe(1);
            }
        }

        [Fact]
        public async Task RollbackAsync_ShouldNotCommit()
        {
            // Arrange
            var tableName = "TestTable_Rollback";
            await using (var setupConn = await SqliteHelper.GetScopedDatabaseConnectionAsync())
            {
                await setupConn.Connection.ExecuteAsync($"CREATE TABLE {tableName} (Id INTEGER PRIMARY KEY, Val TEXT)");
            }

            // Act
            {
                await using var ctx = await DapperCommandContext.NewAsync(SqliteHelper, withTransaction: true);
                await ctx.Connection.ExecuteAsync($"INSERT INTO {tableName} (Val) VALUES ('Test')", transaction: ctx.Transaction);
                await ctx.RollbackAsync();
                // DisposeAsync (via using) should not commit because of rollback
            }

            // Assert
            await using (var assertConn = await SqliteHelper.GetScopedDatabaseConnectionAsync())
            {
                var count = await assertConn.Connection.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM {tableName}");
                count.ShouldBe(0);
            }
        }

        [Fact]
        public async Task Rollback_ShouldNotCommit()
        {
            // Arrange
            var tableName = "TestTable_RollbackSync";
            await using (var setupConn = await SqliteHelper.GetScopedDatabaseConnectionAsync())
            {
                await setupConn.Connection.ExecuteAsync($"CREATE TABLE {tableName} (Id INTEGER PRIMARY KEY, Val TEXT)");
            }

            // Act
            {
                var scopedConn = await SqliteHelper.GetScopedDatabaseConnectionAsync(useTransaction: true);
                using var ctx = new DapperCommandContext(scopedConn.Connection, scopedConn.Transaction, Scope: scopedConn);
                await ctx.Connection.ExecuteAsync($"INSERT INTO {tableName} (Val) VALUES ('Test')", transaction: ctx.Transaction);
                ctx.Rollback();
                // Dispose (via using) should not commit because of rollback
            }

            // Assert
            await using (var assertConn = await SqliteHelper.GetScopedDatabaseConnectionAsync())
            {
                var count = await assertConn.Connection.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM {tableName}");
                count.ShouldBe(0);
            }
        }

        [Fact]
        public async Task Rollback_OnCopiedContext_ShouldPreventCommitOnOriginal()
        {
            // Arrange
            var tableName = "TestTable_RollbackWith";
            await using (var setupConn = await SqliteHelper.GetScopedDatabaseConnectionAsync())
            {
                await setupConn.Connection.ExecuteAsync($"CREATE TABLE {tableName} (Id INTEGER PRIMARY KEY, Val TEXT)");
            }

            // Act
            {
                await using var ctx = await DapperCommandContext.NewAsync(SqliteHelper, withTransaction: true);
                var ctx2 = ctx with { Arguments = new { SomeArg = 1 } };

                await ctx2.Connection.ExecuteAsync($"INSERT INTO {tableName} (Val) VALUES ('Test')", transaction: ctx2.Transaction);

                await ctx2.RollbackAsync();
                // ctx2._wasRolledBack is true.
                // ctx._wasRolledBack is still false (because it was copied before rollback).
                // However, they share the same Transaction.
            }

            // Assert
            await using (var assertConn = await SqliteHelper.GetScopedDatabaseConnectionAsync())
            {
                var count = await assertConn.Connection.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM {tableName}");
                count.ShouldBe(0);
            }
        }
    }
}
