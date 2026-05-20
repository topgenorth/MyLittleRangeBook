using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using MyLittleRangeBook.Console;
using MyLittleRangeBook.Persistence.Sqlite;
using MyLittleRangeBook.Services;

namespace MyLittleRangeBook
{
    [RegisterCommands("firearm")]
    public class FirearmCommands : MlrbCommandBase
    {
        readonly IFirearmsDbService _firearmsDbService;
        readonly FirearmsTablePrinter _printer;
        readonly ISqliteHelper _sqliteHelper;

        public FirearmCommands(ILogger logger,
            ICliDisplay cliDisplay,
            [FromKeyedServices(SqliteHelperExtensions.DI_KEYS_SQLITE)] IFirearmsDbService firearmsDbService,
            ISqliteHelper sqliteHelper) : base(logger, cliDisplay)
        {
            _firearmsDbService = firearmsDbService;
            _sqliteHelper = sqliteHelper;
            _printer = new FirearmsTablePrinter();
        }


        /// <summary>
        ///     List all the active firearms.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Command("list")]
        [UsedImplicitly]
        public async Task<int> PrintFirearmsToConsole(CancellationToken cancellationToken = default)
        {
            AnsiConsole.Console.PrintAppInfo();
            AnsiConsole.Console.WriteLine("Retrieving firearms...");

            await using SqliteConnection conn = await _sqliteHelper
                .GetDatabaseConnectionAsync(cancellationToken)
                .ConfigureAwait(false);
            Result<IEnumerable<Firearm>> firearms = await _firearmsDbService
                .GetFirearmsAsync(conn, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (firearms.IsFailed)
            {
                Logger.Warning("Failed to retrieve firearms.");
                AnsiConsole.Console.PrintProblem("Failed to retrieve firearms.");

                return ReturnCodes.FAILURE;
            }

            if (!firearms.Value.Any())
            {
                AnsiConsole.Console.PrintWarning("No firearms found.");

                return ReturnCodes.SUCCESS;
            }

            _printer.SetFirearms(firearms.Value).Print(AnsiConsole.Console);
            AnsiConsole.Console.PrintSuccess("Firearms retrieved.");

            return ReturnCodes.SUCCESS;
        }
    }
}
