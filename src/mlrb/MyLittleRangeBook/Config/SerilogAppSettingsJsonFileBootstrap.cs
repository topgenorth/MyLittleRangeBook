using System.Text.Json.Nodes;

namespace MyLittleRangeBook.Config
{
    /// <summary>
    ///     Ensures that the appsettings.json file contains a Serilog configuration section. If not, it is created with default
    ///     values for logging to a file and the debug output. The file sink is configured to write logs to the user's local
    ///     application data folder, in a subfolder named "Logs". The log files are rolled daily and have a default output
    ///     template that includes the timestamp, log level, message, and exception details.
    /// </summary>
    public class SerilogAppSettingsJsonFileBootstrap
    {
        /// <summary>
        ///     Default format of the output template when logging to a file.
        /// </summary>
        public const string OUTPUT_TEMPLATE =
            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

        /// <summary>
        ///     This is the Func that will create a default Serilog section in appsettings.json if one does not already exist. It
        ///     is designed to be used as a bootstrapper in the AppSettingsJsonFileBootstrapper.
        /// </summary>
        public static Func<JsonNode?, Result> SerilogSection = rootNode =>
        {
            ArgumentNullException.ThrowIfNull(rootNode);
            if (rootNode["Serilog"] is not null)
            {
                Log.Debug("Serilog configuration already exists in appsettings.json. Skipping Serilog bootstrapper.");

                return Result.Ok();
            }

            try
            {
                string json = ConfigurationExtensions.DefaultSerilogSectionJson().Result;
                JsonNode? serilogSection = JsonNode.Parse(json)?["Serilog"];

                JsonObject? fileSink = serilogSection?["WriteTo"]
                    ?
                    .AsArray()
                    .OfType<JsonObject>()
                    .FirstOrDefault(x => string.Equals(x["Name"]?.GetValue<string>(), "File",
                        StringComparison.OrdinalIgnoreCase));

                if (fileSink is not null)
                {
                    fileSink["Args"] ??= new JsonObject();
                    fileSink["Args"]!["path"] = ConfigurationExtensions.DefaultLogFile;
                }

                rootNode["Serilog"] ??= serilogSection?.DeepClone();
            }
            catch (Exception e)
            {
                return new Result().WithError("Could not create the JSON config for Serilog. " + e.Message);
            }

            return Result.Ok();
        };
    }
}
