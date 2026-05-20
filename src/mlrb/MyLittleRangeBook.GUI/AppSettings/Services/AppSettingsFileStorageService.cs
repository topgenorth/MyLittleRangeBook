using System.IO;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using MyLittleRangeBook.Config;


namespace MyLittleRangeBook.GUI.Services
{
    /// <summary>
    ///     This will read the appsettings.json file from the local disk and return its contents as a string. It will also
    ///     write a string to the appsettings.json file on disk.
    /// </summary>
    public class AppSettingsFileStorageService : ISettingsStorageService
    {
        const string AccentColor = "#FF3578E5";
        const string AppTheme = "Light";
        const string SectionName = "GuiApp";

        const string DefaultSectionJson = """
                                          {
                                            "AccentColor": "#FF3578E5",
                                            "AppTheme": "Light"
                                          }
                                          """;

        /// <summary>
        ///     Check to see if there is a Logging section in the appsettings.json file. If not, create one.
        /// </summary>
        public static readonly Func<JsonNode?, Result> GuiAppSettingsBootstrapper = rootNode =>
        {
            ArgumentNullException.ThrowIfNull(rootNode);

            if (rootNode is not JsonObject rootObject)
            {
                return Result.Fail("Root appsettings JSON must be a JSON object.");
            }

            if (rootObject[SectionName] is null)
            {
                rootObject[SectionName] = JsonNode.Parse(DefaultSectionJson);

                return Result.Ok();
            }

            JsonNode node = rootObject[SectionName]!;
            node["AccentColor"] ??= AccentColor;
            node["AppTheme"] ??= AppTheme;

            return Result.Ok();
        };

        string _settingsFile = ConfigurationExtensions.DefaultAppSettingsFile.FullName;

        public AppSettingsFileStorageService SettingsFile(string settingsFile)
        {
            _settingsFile = settingsFile;

            return this;
        }

        public async Task<string?> ReadAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // Browser has no access to the file system due to sandbox restrictions
                if (OperatingSystem.IsBrowser())
                {
                    return null;
                }

                string originalAppSettingsJson = await File.ReadAllTextAsync(_settingsFile, cancellationToken);
                if (string.IsNullOrEmpty(originalAppSettingsJson))
                {
                    originalAppSettingsJson = "{}";
                }

                var rootNode = JsonNode.Parse(originalAppSettingsJson);
                if (rootNode is null)
                {
                    return null;
                }
                string json = rootNode[SectionName]?.ToJsonString() ?? string.Empty;

                return json;
            }
            catch
            {
                // In production, consider logging any exceptions for debugging
                return null;
            }
        }

        public Task WriteAsync(string json)
        {
            throw new NotImplementedException();
        }
    }
}
