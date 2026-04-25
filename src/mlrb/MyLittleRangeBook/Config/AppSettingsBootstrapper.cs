using System.Text.Json;
using System.Text.Json.Nodes;
using static MyLittleRangeBook.Config.ConfigurationExtensions;

namespace MyLittleRangeBook.Config
{
    /// <summary>
    /// Ensures that the appsettings.json file exists in the user's settings directory.'
    /// </summary>
    public class AppSettingsBootstrapper : IAppSettingsBootstrapper
    {
        const string DefaultAppSettingsJson = """
                                              {
                                                "ConnectionStrings": {
                                                  "SqliteConnection": "Data Source=mlrb.db"
                                                },
                                                "Logging": {
                                                  "LogLevel": {
                                                    "Default": "Error",
                                                    "Microsoft.Hosting.Lifetime": "Error"
                                                  }
                                                }
                                              }
                                              """;




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
        public async Task<string> EnsureAppSettingsExistsAsync(
            CancellationToken cancellationToken = default)
        {
            DefaultUserSettingsDirectory.Create();
            if (DefaultAppSettingsFile.Exists)
            {
                return DefaultAppSettingsFile.FullName;
            }

            string appSettingsFile = DefaultAppSettingsFile.FullName;

            string defaultLogLevel = EnvironmentHelper.IsProduction ? "Error" :
                EnvironmentHelper.IsStaging ? "Debug" : "Verbose";

            var node = JsonNode.Parse(DefaultAppSettingsJson);
            if (node != null)
            {
                // [TO20260414] Just wondering if the Mode should be set to ReadWriteCreate?
                node["ConnectionStrings"]!["SqliteConnection"] = $"Data Source={DefaultSqliteDatabaseName()}";

                JsonNode logLevelNode = node["Logging"]!["LogLevel"]!;
                logLevelNode["Default"] = defaultLogLevel;
                logLevelNode["Microsoft.Hosting.Lifetime"] =
                    EnvironmentHelper.IsProduction ? "Error" : "Warning";

                var options = new JsonSerializerOptions { WriteIndented = true };
                await File.WriteAllTextAsync(appSettingsFile, node.ToJsonString(options), cancellationToken);

            }
            else
            {
                await File.WriteAllTextAsync(appSettingsFile, DefaultAppSettingsJson, cancellationToken);
            }

            return appSettingsFile;
        }
    }
}
