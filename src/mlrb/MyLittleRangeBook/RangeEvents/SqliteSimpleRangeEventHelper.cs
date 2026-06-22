using System.Data;
using System.Runtime.CompilerServices;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook.RangeEvents
{
    public class SqliteSimpleRangeEventHelper : ISimpleRangeEventHelper
    {
        public async Task<Result<(List<string>, List<string>)>> GetFirearmsAndRangesAsync(DapperCommandContext context)
        {
            try
            {
                List<string> firearms =
                    await GetChoicesAsync(context.Connection, Commands.ACTIVE_FIREARMS_SQL, context.CancellationToken)
                         .ToListAsync(context.CancellationToken)
                         .ConfigureAwait(false);
                List<string> ranges =
                    await GetChoicesAsync(context.Connection, Commands.RANGE_NAMES_SQL, context.CancellationToken)
                         .ToListAsync(context.CancellationToken)
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
        /// <param name="context"></param>
        /// <param name="firearmName"></param>
        /// <returns></returns>
        public async Task<Result<List<string>>> GetAmmoDescriptionsForFirearmAsync(DapperCommandContext context,
                                                                                   string                                                                                      firearmName)
        {
            if (string.IsNullOrWhiteSpace(firearmName))
            {
                return Result.Ok(new List<string>());
            }

            try
            {
                await using SqliteCommand command = context.Connection.CreateCommand();
                command.CommandText = Commands.AMMO_FOR_NAMED_FIREARM_SQL;
                command.CommandType = CommandType.Text;
                command.Parameters.AddWithValue("@firearmname", firearmName);

                List<string> ammo = await GetChoicesAsync(command, context.CancellationToken)
                                         .ToListAsync(context.CancellationToken)
                                         .ConfigureAwait(false);

                return Result.Ok(ammo);
            }
            catch (Exception e)
            {
                Result? r = Result.Fail(
                                        new
                                                Error($"Failed to get ammo descriptions for firearm `{firearmName}`: {e.Message}")
                                           .CausedBy(e));

                return r;
            }
        }

        static async IAsyncEnumerable<string> GetChoicesAsync(SqliteCommand command,
                                                              [EnumeratorCancellation]
                                                              CancellationToken cancellationToken)
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
                                                              string           sql,
                                                              [EnumeratorCancellation]
                                                              CancellationToken cancellationToken)
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

        static class Commands
        {
            internal const string RANGE_NAMES_SQL =
                "SELECT DISTINCT range_name FROM simple_range_events ORDER BY modified DESC, range_name;";

            /// <summary>
            ///     Retrieve all the active firearms.
            /// </summary>
            internal const string ACTIVE_FIREARMS_SQL =
                "SELECT DISTINCT firearm_name FROM simple_range_events ORDER BY firearm_name;";

            /// <summary>
            ///     Retrieve all the ammo descriptions for a given firearm name. The results are ordered by the modified date of the
            ///     SimpleRangeEvent, then by the AmmoDescription.
            /// </summary>
            internal const string AMMO_FOR_NAMED_FIREARM_SQL =
                "SELECT DISTINCT ammo_description FROM simple_range_events WHERE firearm_name=@firearmname ORDER BY modified DESC, ammo_description;";
        }
    }
}