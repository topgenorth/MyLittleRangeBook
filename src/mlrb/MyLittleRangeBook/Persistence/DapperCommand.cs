using System.Data;
using Dapper;

namespace MyLittleRangeBook.Persistence
{
    /// <summary>
    ///     Encapsulates a SQL command to be executed using Dapper, including the command text, parameters, and execution
    ///     settings.
    ///     Used for simplifying the creation and execution of SQL commands.
    /// </summary>
    public class DapperCommand
    {
        public DapperCommand(string sql, object? parameters = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(sql);
            CommandType = System.Data.CommandType.Text;
            CommandTimeout = 15;
            Sql = sql;
            Parameters = parameters;
        }

        public string Sql { get; }
        public object? Parameters { get; }
        public CommandType CommandType { get; }
        public int CommandTimeout { get; }

        public CommandDefinition ToDefinition(
            IDbTransaction? transaction = null,
            CancellationToken cancellationToken = default)
        {
            return new CommandDefinition(
                Sql,
                Parameters,
                transaction,
                CommandTimeout,
                CommandType,
                cancellationToken: cancellationToken);
        }

        public async Task<T?> ExecuteScalarAsync<T>(
            IDbConnection connection,
            IDbTransaction? transaction = null,
            CancellationToken cancellationToken = default)
        {
            return await connection.ExecuteScalarAsync<T>(ToDefinition(transaction, cancellationToken));
        }

        public Task<int> ExecuteAsync(
            IDbConnection connection,
            IDbTransaction? transaction = null,
            CancellationToken cancellationToken = default)
        {
            return connection.ExecuteAsync(ToDefinition(transaction, cancellationToken));
        }

        public Task<T?> QuerySingleOrDefaultAsync<T>(
            IDbConnection connection,
            IDbTransaction? transaction = null,
            CancellationToken cancellationToken = default)
        {
            return connection.QuerySingleOrDefaultAsync<T>(ToDefinition(transaction, cancellationToken));
        }

        public Task<T> QuerySingleAsync<T>(
            IDbConnection connection,
            IDbTransaction? transaction = null,
            CancellationToken cancellationToken = default)
        {
            return connection.QuerySingleAsync<T>(ToDefinition(transaction, cancellationToken));
        }

        public Task<IEnumerable<T>> QueryAsync<T>(
            IDbConnection connection,
            IDbTransaction? transaction = null,
            CancellationToken cancellationToken = default)
        {
            return connection.QueryAsync<T>(ToDefinition(transaction, cancellationToken));
        }
    }
}
