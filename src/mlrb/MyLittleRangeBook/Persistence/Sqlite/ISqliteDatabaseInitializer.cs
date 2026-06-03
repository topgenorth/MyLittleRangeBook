namespace MyLittleRangeBook.Persistence.Sqlite
{
    /// <summary>
    /// Defines an interface for initializing and managing an SQLite database, specifically
    /// handling tasks such as applying required database migrations.
    /// </summary>
    public interface ISqliteDatabaseInitializer
    {
        /// <summary>
        ///     Applies the required database migrations to the SQLite database using DbUp.
        /// </summary>
        /// <param name="cancellationToken">
        ///     A token to observe while waiting for the task to complete, allowing the operation to be canceled.
        /// </param>
        /// <returns>
        ///     A result indicating the success or failure of applying the migrations. The result contains a boolean,
        ///     where true indicates the migrations were successfully applied, and false indicates a failure.
        /// </returns>
        Task<Result> ApplyDbupMigrationsAsync(CancellationToken cancellationToken = default);
    }
}
