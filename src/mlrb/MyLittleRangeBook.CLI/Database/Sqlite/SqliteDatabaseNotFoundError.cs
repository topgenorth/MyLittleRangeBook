using FluentResults;

namespace MyLittleRangeBook.CLI.Database.Sqlite
{
    public class SqliteDatabaseNotFoundError : Error
    {
        public SqliteDatabaseNotFoundError(string file) : base($"Could not find the SQLite database '{file}'")
        {
            Metadata.Add("DatabaseFile", file);
        }
    }
}
