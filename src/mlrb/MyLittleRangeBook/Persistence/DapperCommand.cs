using System.Data;
using Dapper;

namespace MyLittleRangeBook.Persistence
{
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
