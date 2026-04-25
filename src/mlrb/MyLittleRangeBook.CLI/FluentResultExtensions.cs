using FluentResults;
using MyLittleRangeBook.CLI.Database.Sqlite;

namespace MyLittleRangeBook.CLI
{
    public static class FluentResultExtensions
    {
        internal static readonly ISuccess DatabaseExists = new Success("Database file exists.");

        internal static Result<int> AssertSqliteDatabaseExists(string databaseFile)
        {
            if (File.Exists(databaseFile))
            {
                return Result.Ok(ReturnCodes.SUCCESS).WithSuccess(DatabaseExists);
            }

            var e = new SqliteDatabaseNotFoundError(databaseFile);

            return Result.Fail<int>(e).WithValue(ReturnCodes.SQL_SQLITE_DATABASE_FILE_NOT_FOUND);
        }
    }
}
