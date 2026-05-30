using System.Data;
using Dapper;

namespace MyLittleRangeBook.Persistence
{
    public interface IDapperCommand
    {
        object? Parameters { get; }
        string Sql { get; }
        CommandType CommandType { get; }
        int CommandTimeout { get; }

        /// <summary>
        ///     Sets the parameters for the command. This can be used to provide parameters after the command has been created,
        ///     allowing for more flexible command construction.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        DapperCommand Arguments(object? p);

        CommandDefinition ToDefinition(DapperCommandContext context)
        {
            return ToDefinition(context.Transaction, context.CancellationToken);
        }

        CommandDefinition ToDefinition(
            IDbTransaction? transaction = null,
            CancellationToken cancellationToken = default);


        Task<T?> ExecuteScalarAsync<T>(
            IDbConnection connection,
            IDbTransaction? transaction = null,
            CancellationToken cancellationToken = default);

        Task<int> ExecuteAsync(
            IDbConnection connection,
            IDbTransaction? transaction = null,
            CancellationToken cancellationToken = default);

        Task<T?> QuerySingleOrDefaultAsync<T>(
            IDbConnection connection,
            IDbTransaction? transaction = null,
            CancellationToken cancellationToken = default);

        Task<T> QuerySingleAsync<T>(
            IDbConnection connection,
            IDbTransaction? transaction = null,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> QueryAsync<T>(
            IDbConnection connection,
            IDbTransaction? transaction = null,
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    ///     Encapsulates a SQL command to be executed using Dapper, including the command text, parameters, and execution
    ///     settings.
    ///     Used for simplifying the creation and execution of SQL commands.
    /// </summary>
    public class DapperCommand : IDapperCommand
    {
        public DapperCommand(string sql, object? parameters = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(sql);
            CommandType = CommandType.Text;
            CommandTimeout = 15;
            Sql = sql;
            Parameters = parameters;
        }

        public object? Parameters { get; private set; }

        public string Sql { get; }


        public CommandType CommandType { get; }
        public int CommandTimeout { get; }

        /// <summary>
        ///     Sets the parameters for the command. This can be used to provide parameters after the command has been created,
        ///     allowing for more flexible command construction.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public DapperCommand Arguments(object? p)
        {
            ArgumentNullException.ThrowIfNull(p);
            Parameters = p;

            return this;
        }

        public CommandDefinition ToDefinition(
            IDbTransaction? transaction = null,
            CancellationToken cancellationToken = default)
        {
            return new CommandDefinition(
                Sql,
                Parameters ?? new { },
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
