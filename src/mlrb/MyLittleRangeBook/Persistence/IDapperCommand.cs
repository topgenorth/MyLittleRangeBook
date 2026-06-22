using System.Data;
using Dapper;

namespace MyLittleRangeBook.Persistence
{
    public interface IDapperCommand
    {
        object?     Parameters     { get; }
        string      Sql            { get; }
        CommandType CommandType    { get; }
        int         CommandTimeout { get; }

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
            IDbTransaction?   transaction       = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Executes the command as a scalar query, returning a single value of type T. The execution is performed using the
        ///     provided context, which includes the database connection, transaction, and cancellation token.
        /// </summary>
        /// <param name="context"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<T?> ExecuteScalarAsync<T>(DapperCommandContext context);

        Task<int> ExecuteAsync(DapperCommandContext ctx);

        Task<T?> QuerySingleOrDefaultAsync<T>(DapperCommandContext ctx);

        Task<T> QuerySingleAsync<T>(DapperCommandContext ctx);

        Task<IEnumerable<T>> QueryAsync<T>(DapperCommandContext ctx);
    }
}