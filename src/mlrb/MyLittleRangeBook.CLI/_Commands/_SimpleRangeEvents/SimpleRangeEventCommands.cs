using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Services;
using static MyLittleRangeBook.CLI.ReturnCodes;
using static MyLittleRangeBook.Database.Sqlite.SqliteHelperExtensions;


namespace MyLittleRangeBook.CLI.Console
{
    [RegisterCommands("rangetrip")]
    public class SimpleRangeEventCommands
    {
        readonly ICliDisplay _cliDisplay;
        readonly ILogger _logger;
        readonly ISimpleRangeEventListPrinter _printer;
        readonly ISimpleRangeEventRepository _repo;

        public SimpleRangeEventCommands(ICliDisplay cliDisplay,
            ILogger logger,
            [FromKeyedServices(DI_KEYS_SQLITE)] ISimpleRangeEventRepository repo,
            ISimpleRangeEventListPrinter printer)
        {
            _cliDisplay = cliDisplay;
            _repo = repo;
            _printer = printer;
            _logger = logger;
        }


        [Command("list")]
        [UsedImplicitly]
        public async Task<int> ListRangeEvents(CancellationToken cancellationToken)
        {
            _cliDisplay.PrintAppInfo();
            Result<IEnumerable<SimpleRangeEvent>> rangeEvents = await _repo.GetSimpleRangeEventsAsync(cancellationToken)
                .ConfigureAwait(true);
            if (rangeEvents.IsFailed)
            {
                _cliDisplay.PrintFailure("Could not retrieve the list.");
                _logger.Warning("Failed to retrieve list from database.");

                return FAILURE;
            }

            _printer.Start();
            foreach (SimpleRangeEvent sre in rangeEvents.Value)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.Warning("Operation cancelled by user.");
                    _cliDisplay.PrintFailure("Operation cancelled.");
                    _printer.Finish();

                    return COMMAND_CANCELLED;
                }

                _printer.AddRow(sre);
            }

            _printer.Finish();

            return SUCCESS;
        }
    }
}
