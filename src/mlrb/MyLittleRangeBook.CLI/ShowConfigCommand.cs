using ConsoleAppFramework;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using MyLittleRangeBook.CLI.Console;
using Spectre.Console;

namespace MyLittleRangeBook.CLI
{
    [RegisterCommands("config")]
    public class ShowConfigCommand
    {
        readonly ICliDisplay _cliDisplay;
        readonly IConfiguration _configuration;

        public ShowConfigCommand(ICliDisplay cliDisplay, IConfiguration configuration)
        {
            _cliDisplay = cliDisplay;
            _configuration = configuration;
        }

        [Command("show")]
        [UsedImplicitly]
        public async Task<int> ShowConfigAsync(CancellationToken cancellationToken = default)
        {
            _cliDisplay.WriteHeader("Show Configuration");

            var table = new Table();
            table.AddColumn("Key");
            table.AddColumn("Value");

            foreach (KeyValuePair<string, string?> pair in _configuration.AsEnumerable().OrderBy(pair => pair.Key))
            {
                table.AddRow(
                    Markup.Escape(pair.Key),
                    Markup.Escape(pair.Value ?? string.Empty));
            }

            _cliDisplay.Console.Write(table);

            return ReturnCodes.SUCCESS;
        }
    }
}
