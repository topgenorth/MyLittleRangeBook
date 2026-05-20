using System.Text.Json.Nodes;

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
        /// <param name="appSettingsJsonFile">The path to the appsettings.json file</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Result> EnsureAppSettingsExistsAsync(string appSettingsJsonFile,
            CancellationToken cancellationToken = default);

        /// <summary>
        ///     Add a Func that will do some work on the appsettings JSON when it is created.
        /// </summary>
        /// <param name="bootstrapper"></param>
        /// <returns></returns>
        IAppSettingsBootstrapper AddBootStrapper(Func<JsonNode?, Result> bootstrapper);
    }
}
