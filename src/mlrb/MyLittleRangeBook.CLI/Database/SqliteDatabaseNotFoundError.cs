namespace MyLittleRangeBook.Database
{
    public class SqliteDatabaseNotFoundError : MlrbBaseError
    {
        public SqliteDatabaseNotFoundError(string file) : base($"Could not find the SQLite database '{file}'",
            ReturnCodes.SQL_SQLITE_DATABASE_FILE_NOT_FOUND)
        {
            Metadata.Add("DatabaseFile", file);
        }
    }
}
