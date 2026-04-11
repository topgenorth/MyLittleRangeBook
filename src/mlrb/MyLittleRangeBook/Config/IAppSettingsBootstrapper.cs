namespace MyLittleRangeBook.Config
{
    public interface IAppSettingsBootstrapper
    {
        /// <summary>
        ///     Ensures that the appsettings.json file exists in the user's settings directory. If it
        ///     does not, then it is created with default values.
        /// </summary>
        /// <remarks>
        ///     In the case of a staging or development environment, the filename will have the
        ///     environment name appended to it.
        /// </remarks>
        /// <param name="cancellationToken"></param>
        /// <returns>The name of the appsettings.json file.</returns>
        Task<string> EnsureAppSettingsExistsAsync(
            CancellationToken cancellationToken = default);
    }
}
