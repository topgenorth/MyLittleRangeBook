using System.Text.Json.Nodes;
using FluentResults;

namespace MyLittleRangeBook.Config
{
    /// <summary>
    ///     Ensures that the appsettings.json file exists in the user's settings directory.'
    /// </summary>
    public class AppSettingsJsonFileBootstrapper : IAppSettingsBootstrapper
    {
        public static readonly List<Func<JsonNode?, Result>?> DefaultBootStrappers = new List<Func<JsonNode?, Result>?>()
        {
            LoggingSectionBootstrapper
        };

        List<Func<JsonNode?, Result>?> _bootstrappers = new List<Func<JsonNode?, Result>?>();

        /// <summary>
        /// Check to see if there is a Logging section in the appsettings.json file. If not, create one.
        /// </summary>
        public static readonly Func<JsonNode?, Result> LoggingSectionBootstrapper = (JsonNode? rootNode) =>
        {
            if (rootNode is not JsonObject rootObject)
            {
                return Result.Fail("Root appsettings JSON must be a JSON object.");
            }

            if (rootObject["Logging"] is null)
            {
                rootObject["Logging"] = JsonNode.Parse(LoggingSectionJson);
            }

            return Result.Ok();
        };

        const string LoggingSectionJson = """
                                          {
                                            "LogLevel": {
                                              "Default": "Error",
                                              "Microsoft.Hosting.Lifetime": "Error"
                                            }
                                          }
                                          """;

        const string DefaultAppSettingsJson = """
                                              {
                                                "ConnectionStrings": {
                                                  "SqliteConnection": "Data Source=mlrb.db"
                                                },

                                              }
                                              """;


        public AppSettingsJsonFileBootstrapper()
        {
        }

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

            foreach (Result? r2 in _bootstrappers.Select(validator => validator!(appSettingsRoot)).Where(r2 => r2.IsFailed))
            {
                return r2;
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
            if (bootstrapper is null)
            {
                throw new ArgumentNullException(nameof(bootstrapper));
            }

            if (_bootstrappers.Contains(bootstrapper))
            {
                return this;
            }
            _bootstrappers.Add(bootstrapper);
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
