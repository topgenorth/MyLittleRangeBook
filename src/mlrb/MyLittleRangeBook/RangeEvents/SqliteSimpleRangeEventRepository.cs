using System.Data;
using System.Data.Common;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using MyLittleRangeBook.Persistence.Sqlite;
using MyLittleRangeBook.RangeEvent;
using static MyLittleRangeBook.Persistence.Sqlite.SqliteHelperExtensions;

namespace MyLittleRangeBook.RangeEvents
{
    /// <summary>
    ///     Represents a repository implementation for managing simple range events using SQLite as the persistence layer.
    /// </summary>
    public class SqliteSimpleRangeEventRepository : ISimpleRangeEventRepository
    {
        // TODO [TO20260505] Introduce SQLite transactions.
        readonly ISimpleRangeEventService _simpleRangeEventService;
        readonly ISqliteHelper _sqliteHelper;

        public SqliteSimpleRangeEventRepository(ISqliteHelper sqliteHelper,
            [FromKeyedServices(DI_KEYS_SQLITE)] ISimpleRangeEventService simpleRangeEventService
        )
        {
            _sqliteHelper = sqliteHelper;
            _simpleRangeEventService = simpleRangeEventService;
        }

        /// <summary>
        ///     Will add or update a simple range event. If necessary, then a new Firearm record will be added.
        /// </summary>
        /// <param name="simpleRangeEvent"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Result<long?>> UpsertAsync(SimpleRangeEvent simpleRangeEvent,
            CancellationToken cancellationToken = default)
        {
            // TODO [TO20260524] This will have to be refactored once we introduce transactions, since we will want to
            // upsert the SRE and the round counts per firearm, where applicable.
            return await UpsertAsync(simpleRangeEvent, Array.Empty<byte>(), cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<Result<IEnumerable<SimpleRangeEvent>>> GetSimpleRangeEventsAsync(
            CancellationToken cancellationToken = default)
        {
            await using SqliteConnection conn = await _sqliteHelper.GetDatabaseConnectionAsync(cancellationToken);

            return await _simpleRangeEventService.GetSimpleRangeEventsAsync(conn, cancellationToken);
        }

        public async Task<Result<SimpleRangeEvent>> GetAsync(string id, CancellationToken cancellationToken)
        {
            const string SQL = "SELECT * FROM main.SimpleRangeEvents WHERE Id=@Id;";
            await using SqliteConnection conn = await _sqliteHelper
                .GetDatabaseConnectionAsync(cancellationToken)
                .ConfigureAwait(false);

            try
            {
                SimpleRangeEvent? sre = await conn.QueryFirstOrDefaultAsync<SimpleRangeEvent>(SQL, new { Id = id });
                if (sre is not null)
                {
                    return Result.Ok(sre);
                }

                Error err = new Error("Could not find range event " + id + ".").Enrich(id);

                return Result.Fail(err);
            }
            catch (Exception ex)
            {
                Error err = new Error(ex.Message).CausedBy(ex).Enrich(id);

                return Result.Fail(err);
            }
        }

        public async Task<Result<bool>> DeleteAsync(SimpleRangeEvent simpleRangeEvent,
            CancellationToken cancellationToken = default)
        {
            await using SqliteConnection conn = await _sqliteHelper.GetDatabaseConnectionAsync(cancellationToken);

            return await _simpleRangeEventService.DeleteAsync(conn, simpleRangeEvent, cancellationToken);
        }


        public async Task<Result<long?>> UpsertAsync(SimpleRangeEvent simpleRangeEvent,
            byte[] fitFileContents,
            CancellationToken cancellationToken = default)
        {
            return await UpsertAsync(simpleRangeEvent, fitFileContents, string.Empty, string.Empty, string.Empty,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<Result<long?>> UpsertAsync(SimpleRangeEvent simpleRangeEvent,
            byte[] fitFileContents,
            string shotViewCsvContents,
            string shotViewFileName,
            string imageFilePath = "",
            CancellationToken cancellationToken = default)
        {
            await using SqliteConnection conn = await _sqliteHelper.GetDatabaseConnectionAsync(cancellationToken);
            await using DbTransaction trans = await conn.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
            Result<long?> finalResult;
            try
            {
                Result<long?> sreResult = await _simpleRangeEventService
                    .UpsertAsync(conn, simpleRangeEvent, cancellationToken)
                    .ConfigureAwait(false);
                return sreResult.IsFailed ? sreResult : sreResult;

                ;
            }
            catch (Exception e)
            {
                Error? err = new Error("Failed to upsert simple range event with file contents.")
                    .Enrich(simpleRangeEvent.Id!, simpleRangeEvent.RowId)
                    .CausedBy(e);
                finalResult = Result.Fail<long?>(err);
            }

            return finalResult;
        }
    }
}
