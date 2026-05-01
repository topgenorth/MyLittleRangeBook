using System.Text.Json.Nodes;
using FluentResults;
using Serilog;

namespace MyLittleRangeBook.Config
{
    /// <summary>
    ///     Ensures that the appsettings.json file contains a Serilog configuration section. If not, it is created with default
    ///     values for logging to a file and the debug output. The file sink is configured to write logs to the user's local
    ///     application data folder, in a subfolder named "Logs". The log files are rolled daily and have a default output
    ///     template that includes the timestamp, log level, message, and exception details.
    /// </summary>
    public class SerilogAppSettingsJsonFileBootstrapp
    {
        /// <summary>
        ///     Default format of the output template when logging to a file.
        /// </summary>
        public const string OutputTemplate =
            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}";

        /// <summary>
        ///     This is the Func that will create a default Serilog section in appsettings.json if one does not already exist. It
        ///     is designed to be used as a bootstrapper in the AppSettingsJsonFileBootstrapper.
        /// </summary>
        public static Func<JsonNode?, Result> EnsureSerilogSection = rootNode =>
        {
            ArgumentNullException.ThrowIfNull(rootNode);
            const string SERILOG_SECTION_JSON = """
                                                "Serilog": {
                                                  "Using": [
                                                    "Serilog.Sinks.Debug",
                                                    "Serilog.Sinks.Console",
                                                    "Serilog.Sinks.File",
                                                    "Serilog.Enrichers.Environment",
                                                    "Serilog.Enrichers.Thread"
                                                  ],
                                                  "MinimumLevel": {
                                                    "Default": "Verbose",
                                                    "Override": {
                                                      "Microsoft": "Warning",
                                                      "System": "Warning"
                                                    }
                                                  },
                                                  "Enrich": [
                                                    "FromLogContext",
                                                    "WithMachineName",
                                                    "WithThreadId"
                                                  ],
                                                  "Properties": {
                                                    "Application": "MyLittleRangeBook.Cli"
                                                  },
                                                  "WriteTo": [
                                                    {
                                                      "Name": "Console"
                                                    },
                                                    {
                                                      "Name": "Debug"
                                                    },
                                                    {
                                                      "Name": "File",
                                                      "Args": {
                                                        "path": "%APPDATA%/MyLittleRangeBook/Logs/mlrb-.log",
                                                        "rollingInterval": "Day",
                                                        "retainedFileCountLimit": 7,
                                                        "shared": true,
                                                        "buffered": false,
                                                        "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}"
                                                      }
                                                    }
                                                  ]
                                                }
                                                """;
            if (rootNode["Serilog"] is not null)
            {
                Log.Debug("Serilog configuration already exists in appsettings.json. Skipping Serilog bootstrapper.");

                return Result.Ok();
            }

            try
            {
                rootNode["Serilog"] ??= JsonNode.Parse(SERILOG_SECTION_JSON);
            }
            catch (Exception e)
            {
                return new Result().WithError("Could not create the JSON config for Serilog. " + e.Message);
            }

            return Result.Ok();
        };
    }
}
