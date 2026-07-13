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

        public object? Parameters { get; private set; }

        public string Sql { get; }


        public CommandType CommandType { get; }
        public int CommandTimeout { get; }



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
