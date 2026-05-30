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

        /// <summary>
        ///     Converts the current DapperCommand instance into a Dapper CommandDefinition object using the provided context.
        /// </summary>
        /// <param name="context">
        ///     The context containing details such as the database connection, transaction, and cancellation token.
        /// </param>
        /// <returns>
        ///     A CommandDefinition object representing the current command with the information derived from the context.
        /// </returns>
        CommandDefinition ToDefinition(DapperCommandContext context)
        {
            return ToDefinition(context.Transaction, context.CancellationToken);
        }

        /// <summary>
        ///     Converts the current DapperCommand instance into a Dapper CommandDefinition object using the provided transaction
        ///     and cancellation token.
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        CommandDefinition ToDefinition(
            IDbTransaction? transaction = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Executes the command as a scalar query, returning a single value of type T. The execution is performed using the
        ///     provided context, which includes the database connection, transaction, and cancellation token.
        /// </summary>
        /// <param name="context"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<T?> ExecuteScalarAsync<T>(DapperCommandContext context);

        [Obsolete("Prefer the DapperCommandContext overload")]
        Task<T?> ExecuteScalarAsync<T>(IDbConnection connection,
            IDbTransaction? transaction = null,
            CancellationToken cancellationToken = default)
        {
            var ctx = new DapperCommandContext(connection, transaction, cancellationToken);

            return ExecuteScalarAsync<T>(ctx);
        }

        Task<int> ExecuteAsync(DapperCommandContext ctx);

        [Obsolete("Prefer the DapperCommandContext overload")]
        Task<int> ExecuteAsync(
            IDbConnection connection,
            IDbTransaction? transaction = null,
            CancellationToken cancellationToken = default)
        {
            var ctx = new DapperCommandContext(connection, transaction, cancellationToken);

            return ExecuteAsync(ctx);
        }

        Task<T?> QuerySingleOrDefaultAsync<T>(DapperCommandContext ctx);

        [Obsolete("Prefer the DapperCommandContext overload")]
        Task<T?> QuerySingleOrDefaultAsync<T>(
            IDbConnection connection,
            IDbTransaction? transaction = null,
            CancellationToken cancellationToken = default)
        {
            var ctx = new DapperCommandContext(connection, transaction, cancellationToken);

            return QuerySingleOrDefaultAsync<T>(ctx);
        }

        Task<T> QuerySingleAsync<T>(DapperCommandContext ctx);

        [Obsolete("Prefer the DapperCommandContext overload")]
        Task<T> QuerySingleAsync<T>(
            IDbConnection connection,
            IDbTransaction? transaction = null,
            CancellationToken cancellationToken = default)
        {
            var ctx = new DapperCommandContext(connection, transaction, cancellationToken);

            return QuerySingleAsync<T>(ctx);
        }

        Task<IEnumerable<T>> QueryAsync<T>(DapperCommandContext ctx);

        [Obsolete("Prefer the DapperCommandContext overload")]
        Task<IEnumerable<T>> QueryAsync<T>(
            IDbConnection connection,
            IDbTransaction? transaction = null,
            CancellationToken cancellationToken = default)
        {
            var ctx = new DapperCommandContext(connection, transaction, cancellationToken);

            return QueryAsync<T>(ctx);
        }
    }

    /// <summary>
    ///     Encapsulates a SQL command to be executed using Dapper, including the command text, parameters, and execution
    ///     settings.
    ///     Used for simplifying the creation and execution of SQL commands.
    /// </summary>
    public class DapperCommand : IDapperCommand
    {
        public DapperCommand(string sql)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(sql);
            CommandType = CommandType.Text;
            CommandTimeout = 15;
            Sql = sql;
        }
        [Obsolete("Pass the parameters in using DapperCommandContext")]
        public DapperCommand(string sql, object? parameters = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(sql);
            CommandType = CommandType.Text;
            CommandTimeout = 15;
            Sql = sql;
            Parameters = parameters;
        }

        [Obsolete("Pass in the arguments using the DapperCommandContext.")]
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
        [Obsolete("Pass in the arguments using the DapperCommandContext.")]
        public DapperCommand Arguments(object? p)
        {
            ArgumentNullException.ThrowIfNull(p);
            Parameters = p;

            return this;
        }

        public CommandDefinition ToDefinition(DapperCommandContext ctx)
        {
            return new CommandDefinition(
                Sql,
                ctx.Arguments?? new { },
                ctx.Transaction,
                CommandTimeout,
                CommandType,
                cancellationToken: ctx.CancellationToken);
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

        public async Task<T?> ExecuteScalarAsync<T>(DapperCommandContext ctx)
        {
            return await ctx.Connection.ExecuteScalarAsync<T>(ToDefinition(ctx));
        }

        public Task<int> ExecuteAsync(DapperCommandContext ctx)
        {
            return ctx.Connection.ExecuteAsync(ToDefinition(ctx));
        }

        public Task<T?> QuerySingleOrDefaultAsync<T>(DapperCommandContext ctx)
        {
            return ctx.Connection.QuerySingleOrDefaultAsync<T>(ToDefinition(ctx));
        }

        public Task<T> QuerySingleAsync<T>(DapperCommandContext ctx)
        {
            return ctx.Connection.QuerySingleAsync<T>(ToDefinition(ctx));
        }

        public Task<IEnumerable<T>> QueryAsync<T>(DapperCommandContext ctx)
        {
            return ctx.Connection.QueryAsync<T>(ToDefinition(ctx));
        }
    }
}
