using System.Text.Json.Nodes;
using ConsoleAppFramework;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using MyLittleRangeBook.Console;
using ConfigurationExtensions = MyLittleRangeBook.Config.ConfigurationExtensions;

namespace MyLittleRangeBook
{
    [RegisterCommands("config")]
    public class ConfigurationCommands: MlrbCommandBase
    {
        readonly IConfiguration _configuration;

        public ConfigurationCommands(ILogger logger, ICliDisplay cliDisplay, IConfiguration configuration) : base(logger, cliDisplay)
        {
            _configuration = configuration;
        }

        /// <summary>
        ///     Used to set the path to the SQLite datbase
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [Command("set db")]
        [UsedImplicitly]
        public async Task<int> SetDatabasePath(string connectionString, CancellationToken ct = default)
        {
            CliDisplay.PrintCommandHeader("Set Database Path");
            string appSettingsJsonFile = ConfigurationExtensions.DefaultAppSettingsFile.FullName;
            string originalAppSettingsJson;
            try
            {
                originalAppSettingsJson = await File.ReadAllTextAsync(appSettingsJsonFile, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to read appsettings.json file.");
                CliDisplay.PrintFailure("Could not read the settings file.");
                return ReturnCodes.FAILURE;
            }

            var rootNode = JsonNode.Parse(originalAppSettingsJson);
            if (rootNode is null)
            {
                Logger.Error("Failed to parse appsettings.json file.");
                CliDisplay.PrintFailure("Could not parse the settings file.");
                return ReturnCodes.FAILURE;
            }

            var sb = new SqliteConnectionStringBuilder
            {
                DataSource = connectionString, Mode = SqliteOpenMode.ReadWriteCreate
            };

            try
            {
                rootNode["ConnectionStrings"]!["SqliteConnection"] = sb.ConnectionString;

                await File.WriteAllTextAsync(appSettingsJsonFile, rootNode!.ToString(), ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to write appsettings.json file.");
                CliDisplay.PrintFailure("Failed to write to the settings file.");
                return ReturnCodes.FAILURE;
            }

            CliDisplay.PrintSuccess("Updated path to SQLite database in appsettings.json file.");

            return ReturnCodes.SUCCESS;
        }

        [Command("show")]
        [UsedImplicitly]
        // ReSharper disable once AsyncMethodWithoutAwait
        public async Task<int> ShowConfigAsync(CancellationToken cancellationToken = default)
        {
            Logger.Verbose("Showing configuration values.");
            CliDisplay.PrintCommandHeader("Show Configuration");

            Table table = new Table().BorderColor(Color.White).Expand();
            table.AddColumn("Key");
            table.AddColumn("Value");

            foreach (KeyValuePair<string, string?> pair in _configuration.AsEnumerable().OrderBy(pair => pair.Key))
            {
                table.AddRow(
                    Markup.Escape(pair.Key),
                    Markup.Escape(pair.Value ?? string.Empty));
            }

            CliDisplay.Console.Write(table);
            CliDisplay.PrintSuccess("Configuration displayed.");

            return ReturnCodes.SUCCESS;
        }
    }
}
