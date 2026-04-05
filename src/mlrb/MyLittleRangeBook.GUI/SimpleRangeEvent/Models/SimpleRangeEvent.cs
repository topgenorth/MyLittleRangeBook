using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using NanoidDotNet;
using Serilog;

namespace MyLittleRangeBook.GUI.Models
{
    public record SimpleRangeEvent
    {
        /// <summary>
        ///     A Nanoid to uniquely identify the SimpleRangeEvent. Will be null for a new entity.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        ///     The database row ID of the SimpleRangeEvent. Will be null for a new record.
        /// </summary>
        public long? RowId { get; set; }

        /// <summary>
        ///     The date that the event took place.
        /// </summary>
        public DateTime EventDate { get; set; }

        /// <summary>
        ///     The name of the firearm used. Should match a firearm in the Firearms table.
        /// </summary>
        public string FirearmName { get; set; } = string.Empty;

        /// <summary>
        ///     The name of the range the event took place.
        /// </summary>
        public string RangeName { get; set; } = string.Empty;

        /// <summary>
        ///     How many rounds were fired.
        /// </summary>
        public int RoundsFired { get; set; }

        /// <summary>
        ///     The description of the ammo used.
        /// </summary>
        public string? AmmoDescription { get; set; }

        /// <summary>
        ///     Any additional notes about the event.
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        ///     The time (UTC) that the record was created.
        /// </summary>
        public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        ///     The time (UTC) that the record was last modified.
        /// </summary>
        public DateTimeOffset Modified { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        ///     Will upsert the SimpleRangeEvent into the database.
        /// </summary>
        /// <remarks>
        ///     This requires that the SQLite connection has a function registered called nanoid().
        /// </remarks>
        /// <returns></returns>
        public async Task<bool> SaveAsync(SqliteConnection connection, CancellationToken cancellationToken = default)
        {
            Modified = DateTimeOffset.UtcNow;
            try
            {
                Id ??= await Nanoid.GenerateAsync();

                if (RowId is null)
                {
                    RowId = await connection.QuerySingleAsync<long>(
                        """
                        INSERT INTO SimpleRangeEvents (Id, EventDate, FirearmName, RangeName, RoundsFired, AmmoDescription, Notes, Created, Modified)
                        VALUES (nanoid(), @EventDate, @FirearmName, @RangeName, @RoundsFired, @AmmoDescription, @Notes, @Created, @Modified)
                        RETURNING RowId;
                        """,
                        this);
                }
                else
                {
                    await connection.ExecuteAsync(
                        """
                        UPDATE SimpleRangeEvents 
                        SET EventDate = @EventDate, FirearmName = @FirearmName, RangeName = @RangeName, 
                            RoundsFired = @RoundsFired, AmmoDescription = @AmmoDescription, Notes = @Notes, 
                            Modified = @Modified
                        WHERE Id = @Id;
                        """,
                        this);
                }
                return RowId != null;
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Could not save SimpleRangeEvent `{Id}`", RowId);
                Trace.TraceError(e.Message);

                return false;
            }
        }

        public async Task<bool> DeleteAsync(SqliteConnection connection, CancellationToken cancellationToken = default)
        {
            if (RowId is null)
            {
                return false;
            }

            try
            {
                await connection.ExecuteAsync("DELETE FROM SimpleRangeEvents WHERE Id = @Id;", this);
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Failed to delete SimpleRangeEvent `{0}`", RowId);
                Trace.TraceError(e.Message);

                return false;
            }

            return true;
        }
    }
}
