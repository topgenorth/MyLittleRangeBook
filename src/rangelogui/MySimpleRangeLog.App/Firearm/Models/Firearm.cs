using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using MyLittleRangeBook.Gui.Database;
using MyLittleRangeBook.Gui.Services;
using NanoidDotNet;
using Serilog;

namespace MyLittleRangeBook.Gui.Models
{
    public record Firearm
    {
        /// <summary>
        /// A Nanoid to uniquely identify the Firearm. Will be null for a new entity.
        /// </summary>
        public string? Id { get; set; }
        
        /// <summary>
        /// The database row ID of the Firearm. Will be null for a new record.
        /// </summary>
        public long? RowId { get; set; }
        /// <summary>
        /// The common name of the Firearm.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        public string? Notes { get; set; }
        /// <summary>
        /// The time (UTC) that the record was created.
        /// </summary>
        public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;
        /// <summary>
        /// The time (UTC) that the record was last modified.
        /// </summary>
        public DateTimeOffset Modified { get; set; } = DateTimeOffset.UtcNow;

        public async Task<bool> DeleteAsync(SqliteConnection connection, CancellationToken cancellationToken = default)
        {
            if (RowId is null)
            {
                return false;
            }

            try
            {
                await connection.ExecuteAsync("DELETE FROM Firearms WHERE Id = @Id;", this);
                await DatabaseHelper.SyncUnderlyingDatabaseAsync();
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Failed to delete firearm {Id}", RowId);
                Trace.TraceError(e.Message);

                return false;
            }

            return true;
        }

        /// <summary>
        /// Will upsert the Firearm into the database.
        /// </summary>
        /// <remarks>
        ///     This requires that the SQLite connection has a function registered called nanoid().
        /// </remarks> 
        /// <returns></returns>
        public async Task<bool> SaveAsync(SqliteConnection db, CancellationToken cancellationToken = default)
        {
            Modified = DateTimeOffset.UtcNow;
            var dbService = App.Services.GetRequiredService<IDatabaseService>();

            try
            {
                Id ??= await Nanoid.GenerateAsync();
                if (RowId is null)
                {
                    RowId = await db.QuerySingleAsync<long>("""
                                                         INSERT INTO Firearms (Id, Name, Notes) 
                                                         VALUES (nanoid(), @Name, @Notes) 
                                                         RETURNING RowId
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

                return RowId != null;
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Could not save Firearm `{Id}`", RowId);

                return false;
            }
        }

        public override string ToString()
        {
            return $"{Id ?? "N/A"}{Name}";
        }
    }
}
