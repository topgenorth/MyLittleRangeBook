using System.Data;

namespace MyLittleRangeBook.Persistence
{
    /// <summary>
    ///    Provides a context for database operations, encapsulating the necessary components such as the database connection.
    /// </summary>
    /// <param name="Connection"></param>
    /// <param name="Transaction"></param>
    /// <param name="CancellationToken"></param>
    public record DapperCommandContext(IDbConnection Connection, IDbTransaction? Transaction = null, CancellationToken CancellationToken = default);
}
