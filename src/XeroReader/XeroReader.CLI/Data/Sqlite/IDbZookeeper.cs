namespace net.opgenorth.xero.Data.Sqlite
{
    public interface IDbZookeeper
    {
        string ConnectionString { get; }
        string SqliteFile { get; }

        /// <summary>
        ///     Runs the migrations on the <b>.sqlite</b> file.
        /// </summary>
        void UpdateDatabase();

        /// <summary>
        ///     Will delete the <b>.sqlite</b> file if it exists, create a new one, and apply migrations.
        /// </summary>
        void CreateDatabase();

        int GetHashCode();
        string ToString();
    }
}
