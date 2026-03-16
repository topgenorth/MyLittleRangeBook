using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using MySimpleRangeLog.Helper;
using MySimpleRangeLog.Services;
using Serilog;

namespace MySimpleRangeLog.Models
{
    public record SimpleRangeEvent
    {
        public long? Id { get; set; }
        public DateTime EventDate { get; set; }
        public string FirearmName { get; set; } = string.Empty;
        public string RangeName { get; set; } = string.Empty;
        public int RoundsFired { get; set; }
        public string? AmmoDescription { get; set; }
        public string? Notes { get; set; }
        public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset Modified { get; set; } = DateTimeOffset.UtcNow;

        public async Task<bool> SaveAsync()
        {
            Modified = DateTimeOffset.UtcNow;
            var dbService = App.Services.GetRequiredService<IDatabaseService>();
            try
            {
                await using var connection = await DatabaseHelper.GetOpenConnectionAsync(dbService);
                if (Id is null)
                {
                    // INSERT with RETURNING (SQLite 3.35+ supports this)
                    Id = await connection.QuerySingleAsync<long>(
                        """
                        INSERT INTO SimpleRangeEvents (EventDate, FirearmName, RangeName, RoundsFired, AmmoDescription, Notes, Created, Modified)
                        VALUES (@EventDate, @FirearmName, @RangeName, @RoundsFired, @AmmoDescription, @Notes, @Created, @Modified)
                        RETURNING Id;
                        """,
                        this);
                }
                else
                {
                    // UPDATE
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


                // Id = await connection.ExecuteScalarAsync<long?>(
                //     """
                //     REPLACE INTO SimpleRangeEvents (Id, EventDate, FirearmName, RangeName, RoundsFired, AmmoDescription, Notes, Modified)
                //             VALUES (@Id, @EventDate, @FirearmName, @RangeName, @RoundsFired, @AmmoDescription, @Notes, @Modified);
                //     SELECT Last_insert_rowid();
                //     """, this
                // );

                // Remember to sync the Indexed-DB.
                await DatabaseHelper.SyncUnderlyingDatabaseAsync();

                return Id != null;
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Could not save SimpleRangeEvent `{0}`", Id);
                Trace.TraceError(e.Message);

                return false;
            }
        }

        public async Task<bool> DeleteAsync()
        {
            if (Id is null)
            {
                return false;
            }

            try
            {
                await using var connection = await DatabaseHelper.GetOpenConnectionAsync(App.Services.GetRequiredService<IDatabaseService>());
                await connection.ExecuteAsync("DELETE FROM SimpleRangeEvents WHERE Id = @Id;", this);
                await DatabaseHelper.SyncUnderlyingDatabaseAsync();

                return true;
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Failed to delete SimpleRangeEvent `{0}`", Id);
                Trace.TraceError(e.Message);

                return false;
            }
        }
    }
}
