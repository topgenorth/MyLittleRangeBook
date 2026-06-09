using System.Data;
using System.Runtime.CompilerServices;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook.RangeEvents
{
    public class SqliteSimpleRangeEventHelper : ISimpleRangeEventHelper
    {
        internal const string SQL_RANGE_NAMES =
            "SELECT DISTINCT range_name FROM simple_range_events ORDER BY modified DESC, range_name;";

        /// <summary>
        ///     Retrieve all the active firearms.
        /// </summary>
        internal const string SQL_ACTIVE_FIREARMS =
            "SELECT DISTINCT firearm_name FROM simple_range_events ORDER BY firearm_name;";

        /// <summary>
        ///     Retrieve all the ammo descriptions for a given firearm name. The results are ordered by the modified date of the
        ///     SimpleRangeEvent, then by the AmmoDescription.
        /// </summary>
        internal const string SQL_AMMO_FOR_NAMED_FIREARM =
            "SELECT DISTINCT ammo_description FROM simple_range_events WHERE firearm_name=@firearmname ORDER BY modified DESC, ammo_description;";

        readonly ISqliteHelper _sqliteHelper;

        public SqliteSimpleRangeEventHelper(ISqliteHelper sqliteHelper)
        {
            _sqliteHelper = sqliteHelper;
        }

        public async Task<Result<(List<string>, List<string>)>> GetFirearmsAndRangesAsync(
            CancellationToken cancellationToken)
        {
            SqliteConnection conn;

            try
            {
                conn = await _sqliteHelper.GetDatabaseConnectionAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Error? err = new Error("Failed to open connection to database.").CausedBy(e);

                return Result.Fail<(List<string>, List<string>)>(err).WithValue(([], []));
            }

            try
            {
                List<string> firearms = await GetChoicesAsync(conn, SQL_ACTIVE_FIREARMS, cancellationToken)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
                List<string> ranges = await GetChoicesAsync(conn, SQL_RANGE_NAMES, cancellationToken)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);

                return Result.Ok((firearms, ranges));
            }
            catch (Exception e)
            {
                Error? err = new Error("Failed to retrieve firearm and range names from database.").CausedBy(e);

                return Result.Fail<(List<string>, List<string>)>(err).WithValue(([], []));
            }
        }

        /// <summary>
        ///     Retrieve all the ammo descriptions for a given firearm name. If the firearm is null or empty string, then an empty
        ///     list is returned.
        /// </summary>
        /// <param name="firearmName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<Result<List<string>>> GetAmmoDescriptionsForFirearmAsync(
            string firearmName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(firearmName))
            {
                return Result.Ok(new List<string>());
            }

            try
            {
                await using SqliteConnection conn = await _sqliteHelper.GetDatabaseConnectionAsync(cancellationToken)
                    .ConfigureAwait(false);

                await using SqliteCommand command = conn.CreateCommand();
                command.CommandText = SQL_AMMO_FOR_NAMED_FIREARM;
                command.CommandType = CommandType.Text;
                command.Parameters.AddWithValue("@firearmname", firearmName);

                List<string> ammo = await GetChoicesAsync(command, cancellationToken)
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);

                return Result.Ok(ammo);
            }
            catch (Exception e)
            {
                var r = Result.Fail(
                    new Error($"Failed to get ammo descriptions for firearm `{firearmName}`: {e.Message}")
                        .CausedBy(e));

                return r;
            }
        }

        static async IAsyncEnumerable<string> GetChoicesAsync(SqliteCommand command,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await using SqliteDataReader reader =
                await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                cancellationToken.ThrowIfCancellationRequested();

                yield return reader.GetString(0);
            }
        }

        static async IAsyncEnumerable<string> GetChoicesAsync(SqliteConnection connection,
            string sql,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await using SqliteCommand command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandType = CommandType.Text;
            await using SqliteDataReader reader =
                await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                cancellationToken.ThrowIfCancellationRequested();

                yield return reader.GetString(0);
            }
        }
    }
}
