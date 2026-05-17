using System.Text.Json.Nodes;
using FluentResults;
using Serilog;

namespace MyLittleRangeBook.Config
{
    /// <summary>
    ///     This class holds the functions that are used to bootstrap the CLI and GUI on the first run.
    /// </summary>
    public class BootstrapFuncs
    {
        /// <summary>
        ///     Check to see if there is a Logging section in the appsettings.json file. If not, create one.
        /// </summary>
        public static readonly Func<JsonNode?, Result> LoggingSectionBootstrapper = rootNode =>
        {
            ArgumentNullException.ThrowIfNull(rootNode);

            if (rootNode["Logging"] is not null)
            {
                Log.Debug("Logging configuration already exists in appsettings.json. Skipping Logging bootstrapper.");

                return Result.Ok();
            }

            string json = ConfigurationExtensions.DefaultLoggingSectionJson().Result;
            rootNode["Logging"] ??= JsonNode.Parse(json);

            return Result.Ok();
        };
    }
}
