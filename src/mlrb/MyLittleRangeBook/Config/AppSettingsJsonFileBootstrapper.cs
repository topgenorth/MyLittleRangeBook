using System.Text.Json.Nodes;
using FluentResults;
using Serilog;

namespace MyLittleRangeBook.Config
{
    /// <summary>
    ///     Ensures that the appsettings.json file exists in the user's settings directory.'
    /// </summary>
    public class AppSettingsJsonFileBootstrapper : IAppSettingsBootstrapper
    {
        /// <summary>
        ///     Check to see if there is a Logging section in the appsettings.json file. If not, create one.
        /// </summary>
        public static readonly Func<JsonNode?, Result> LoggingSectionBootstrapper = rootNode =>
        {
            ArgumentNullException.ThrowIfNull(rootNode);
            const string LOGGING_SECTION_JSON = """
                                                {
                                                  "LogLevel": {
                                                    "Default": "Error",
                                                    "Microsoft.Hosting.Lifetime": "Error"
                                                  }
                                                }
                                                """;

            if (rootNode["Logging"] is not null)
            {
                Log.Debug("Logging configuration already exists in appsettings.json. Skipping Logging bootstrapper.");
                return Result.Ok();
            }
            rootNode["Logging"] ??= JsonNode.Parse(LOGGING_SECTION_JSON);

            return Result.Ok();
        };

        readonly List<Func<JsonNode?, Result>?> _bootstrappers = new();

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
                    Log.Warning(e, "EnsureSerilogSection {EnsureSerilogSection} threw an exception.", bootstrapper?.Method.Name);
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

        public IAppSettingsBootstrapper AddBootStrapper(IEnumerable<Func<JsonNode?, Result>?> bootstrappers)
        {
            foreach (Func<JsonNode?, Result>? b in bootstrappers.OfType<Func<JsonNode?, Result>>())
            {
                AddBootStrapper(b);
            }

            return this;
        }

        internal async Task<Result> CreateAppSettingsFile(string pathToFile)
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
                await File.WriteAllTextAsync(file.FullName, "{}");
            }
            catch (Exception ex)
            {
                return new AppSettingsJsonCreationError(file.FullName, ex);
            }

            return Result.Ok();
        }
    }
}
