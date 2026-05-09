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
    [RegisterCommands("rangeevent")]
    public class SimpleRangeEventCommands: MlrbCommandBase
    {
        readonly ISimpleRangeEventListPrinter _printer;
        readonly ISimpleRangeEventRepository _repo;

        public SimpleRangeEventCommands(ILogger logger,
            ICliDisplay cliDisplay,
            [FromKeyedServices(DI_KEYS_SQLITE)] ISimpleRangeEventRepository repo,
            ISimpleRangeEventListPrinter printer): base(logger, cliDisplay)
        {
            _repo = repo;
            _printer = printer;
        }


        /// <summary>
        ///     List all the range events in the database.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Command("list")]
        [UsedImplicitly]
        public async Task<int> ListRangeEvents(CancellationToken cancellationToken)
        {
            CliDisplay.PrintAppInfo();
            Result<IEnumerable<SimpleRangeEvent>> rangeEvents = await _repo.GetSimpleRangeEventsAsync(cancellationToken)
                .ConfigureAwait(false);
            if (rangeEvents.IsFailed)
            {
                CliDisplay.PrintFailure("Could not retrieve the list.");
                Logger.Warning("Failed to retrieve list from database.");

                return FAILURE;
            }

            await _printer.Start().ConfigureAwait(false);
            foreach (SimpleRangeEvent sre in rangeEvents.Value)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Logger.Warning("Operation cancelled by user.");
                    CliDisplay.PrintFailure("Operation cancelled.");
                    await _printer.Finish().ConfigureAwait(false);

                    return COMMAND_CANCELLED;
                }

                await _printer.AddRow(sre).ConfigureAwait(false);
            }

            await _printer.Finish().ConfigureAwait(false);

#if DEBUG
            // [TO20260507] Need this when testing in Rider.  Without it the console window closes too fast.
            System.Console.ReadKey();
#endif

            return SUCCESS;
        }
    }
}
