using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using MySimpleRangeLog.Database;
using MySimpleRangeLog.Services;
using Serilog;

namespace MySimpleRangeLog.Models
{
    public record Firearm
    {
        public long? Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public string? Notes { get; set; }
        public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset Modified { get; set; } = DateTimeOffset.UtcNow;

        public async Task<bool> DeleteAsync()
        {
            if (Id is null)
            {
                return false;
            }

            try
            {
                await using var connection =
                    await DatabaseHelper.GetOpenConnectionAsync(App.Services.GetRequiredService<IDatabaseService>());
                await connection.ExecuteAsync("DELETE FROM Firearms WHERE Id = @Id;", this);
                await DatabaseHelper.SyncUnderlyingDatabaseAsync();
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Failed to delete firearm {Id}", Id);
                Trace.TraceError(e.Message);

                return false;
            }

            return true;
        }

        public async Task<bool> SaveAsync()
        {
            Modified = DateTimeOffset.UtcNow;
            var dbService = App.Services.GetRequiredService<IDatabaseService>();

            try
            {
                await using var db = await DatabaseHelper.GetOpenConnectionAsync(dbService);
                if (Id is null)
                {
                    Id = await db.QuerySingleAsync<long>("""
                                                         INSERT INTO Firearms (Name, Notes) 
                                                         VALUES (@Name, @Notes) 
                                                         RETURNING Id
                                                         """, new { Name, Notes });
                }
                else
                {
                    await db.ExecuteAsync("""
                                          UPDATE Firearms 
                                          SET Name = @Name, Notes = @Note, Modified = @Modified 
                                          WHERE Id = @Id
                                          """, this);
                }

                await DatabaseHelper.SyncUnderlyingDatabaseAsync();

                return Id != null;
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Could not save Firearm `{Id}`", Id);

                return false;
            }
        }
    }
}
