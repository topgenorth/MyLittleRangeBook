using System.Text.Json.Nodes;

namespace MyLittleRangeBook.Config
{
    /// <summary>
    ///     Ensures that the appsettings.json file exists in the user's settings directory.'
    /// </summary>
    public class AppSettingsJsonFileBootstrapper : IAppSettingsBootstrapper
    {
        readonly List<Func<JsonNode?, Result>?> _bootstrappers = [];

        /// <summary>
        ///     Ensures that the appsettings.json file exists in the user's settings directory. If it
        ///     does not, then it is created with default values.
        /// </summary>
        /// <remarks>
        ///     In the case of a staging or development environment, the filename will have the
        ///     environment name appended to it.
        /// </remarks>
        /// <param name="appSettingsJsonFile"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>The full path of the appsettings.json file.</returns>
        public async Task<Result> EnsureAppSettingsExistsAsync(string appSettingsJsonFile,
            CancellationToken cancellationToken = default)
        {
            Result r1 = await CreateAppSettingsFile(appSettingsJsonFile);
            if (r1.IsFailed)
            {
                return r1;
            }

            string originalAppSettingsJson;

            try
            {
                originalAppSettingsJson = await File.ReadAllTextAsync(appSettingsJsonFile, cancellationToken);
            }
            catch (Exception ex)
            {
                return new AppSettingsJsonCreationError(appSettingsJsonFile, ex);
            }

            JsonNode? appSettingsRoot;
            try
            {
                appSettingsRoot = JsonNode.Parse(originalAppSettingsJson);
            }
            catch (Exception ex)
            {
                return new AppSettingsJsonCreationError(appSettingsJsonFile, ex);
            }

            appSettingsRoot ??= JsonNode.Parse("{}")!;

            var error = new Error("Failed to bootstrap appsettings.json file.");
            foreach (Func<JsonNode?, Result>? bootstrapper in _bootstrappers)
            {
                try
                {
                    bootstrapper!(appSettingsRoot);
                }
                catch (Exception e)
                {
                    // TODO [TO20260501] Accumulate all errors in the result.
                    Log.Warning(e, "SerilogSection {SerilogSection} threw an exception.", bootstrapper?.Method.Name);
                    error.CausedBy(e);
                }
            }

            try
            {
                await File.WriteAllTextAsync(appSettingsJsonFile, appSettingsRoot!.ToString(), cancellationToken);
            }
            catch (Exception ex)
            {
                return new AppSettingsJsonCreationError(appSettingsJsonFile, ex);
            }

            return Result.Ok();
        }

        public IAppSettingsBootstrapper AddBootStrapper(Func<JsonNode?, Result> bootstrapper)
        {
            ArgumentNullException.ThrowIfNull(bootstrapper);

            if (_bootstrappers.Contains(bootstrapper))
            {
                return this;
            }

            _bootstrappers.Add(bootstrapper);

            return this;
        }

        internal async Task<Result> CreateAppSettingsFile(string pathToFile, string contents = "{}")
        {
            var file = new FileInfo(pathToFile);
            DirectoryInfo? parentDirectory = file.Directory;
            if (parentDirectory is not null && !parentDirectory.Exists)
            {
                parentDirectory.Create();
            }

            if (file.Exists)
            {
                return Result.Ok();
            }

            try
            {
                await File.WriteAllTextAsync(file.FullName, contents);
            }
            catch (Exception ex)
            {
                return new AppSettingsJsonCreationError(file.FullName, ex);
            }

            return Result.Ok();
        }
    }
}
