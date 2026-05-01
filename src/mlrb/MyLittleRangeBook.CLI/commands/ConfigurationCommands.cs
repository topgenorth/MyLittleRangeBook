using System.Text.Json.Nodes;
using ConsoleAppFramework;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using MyLittleRangeBook.CLI.Console;
using Spectre.Console;

namespace MyLittleRangeBook.CLI
{
    [RegisterCommands("config")]
    public class ConfigurationCommands
    {
        readonly ICliDisplay _cliDisplay;
        readonly IConfiguration _configuration;
        readonly ILogger _logger;

        public ConfigurationCommands(ICliDisplay cliDisplay, IConfiguration configuration, ILogger logger)
        {
            _cliDisplay = cliDisplay;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        ///     Used to set the path to the SQLite datbase
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Command("set db")]
        [UsedImplicitly]
        public async Task<int> SetDatabasePath(
            string connectionString,
            CancellationToken cancellationToken = default)
        {
            _cliDisplay.WriteAppInfo("Set Database Path");
            string appSettingsJsonFile = Config.ConfigurationExtensions.DefaultAppSettingsFile.FullName;
            string originalAppSettingsJson;
            try
            {
                originalAppSettingsJson = await File.ReadAllTextAsync(appSettingsJsonFile, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to read appsettings.json file.");
                return ReturnCodes.FAILURE;
            }

            var rootNode=JsonNode.Parse(originalAppSettingsJson);
            if (rootNode is null)
            {
                _logger.Error("Failed to parse appsettings.json file.");
                return ReturnCodes.FAILURE;
            }

            var sb = new SqliteConnectionStringBuilder()
            {
                DataSource = connectionString,
                Mode = SqliteOpenMode.ReadWriteCreate
            };

            try
            {
                rootNode["ConnectionStrings"]!["SqliteConnection"] = sb.ConnectionString;

                await File.WriteAllTextAsync(appSettingsJsonFile, rootNode!.ToString(), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to write appsettings.json file.");
                return ReturnCodes.FAILURE;
            }

            _cliDisplay.WriteSuccess("Updated path to SQLite database in appsettings.json file.");
            return ReturnCodes.SUCCESS;
        }

        [Command("show")]
        [UsedImplicitly]
        // ReSharper disable once AsyncMethodWithoutAwait
        public async Task<int> ShowConfigAsync(CancellationToken cancellationToken = default)
        {
            _cliDisplay.WriteAppInfo("Show Configuration");

            Table table = new Table().BorderColor(Color.White).Expand();
            table.AddColumn("Key");
            table.AddColumn("Value");

            foreach (KeyValuePair<string, string?> pair in _configuration.AsEnumerable().OrderBy(pair => pair.Key))
            {
                table.AddRow(
                    Markup.Escape(pair.Key),
                    Markup.Escape(pair.Value ?? string.Empty));
            }

            _cliDisplay.Console.Write(table);
            _cliDisplay.WriteSuccess("Configuration displayed.");

            return ReturnCodes.SUCCESS;
        }
    }
}
