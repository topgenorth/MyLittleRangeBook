using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using MyLittleRangeBook.CLI.Console;
using MyLittleRangeBook.Database.Sqlite;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Services;
using Spectre.Console;

namespace MyLittleRangeBook.CLI
{
    [RegisterCommands("firearm")]
    public class FirearmCommands
    {
        readonly ILogger _logger;
        readonly ISqliteHelper _sqliteHelper;
        readonly IFirearmsDbService _firearmsDbService;
        readonly FirearmsTablePrinter _printer;

        public FirearmCommands([FromKeyedServices(SqliteHelperExtensions.DI_KEYS_SQLITE)]IFirearmsDbService firearmsDbService,
            ISqliteHelper sqliteHelper,
            ILogger logger)
        {
            _firearmsDbService = firearmsDbService;
            _sqliteHelper = sqliteHelper;
            _logger = logger;
            _printer = new FirearmsTablePrinter();
        }


        /// <summary>
        /// List all the active firearms.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Command("list")]
        [UsedImplicitly]
        public async Task<int> PrintFirearmsToConsole(CancellationToken cancellationToken = default)
         {
            AnsiConsole.Console.WriteAppInfo();
            AnsiConsole.Console.Write("Retrieving firearms...");

            await using SqliteConnection conn = await _sqliteHelper
                .GetDatabaseConnectionAsync(cancellationToken)
                .ConfigureAwait(false);
            Result<IEnumerable<Firearm>> firearms = await _firearmsDbService
                .GetFirearmsAsync(conn, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (firearms.IsFailed)
            {
                _logger.Warning("Failed to retrieve firearms.");
                AnsiConsole.Console.WriteProblem("Failed to retrieve firearms.");
                return ReturnCodes.FAILURE;
            }

            if (!firearms.Value.Any())
            {
                AnsiConsole.Console.WriteWarning("No firearms found.");
                return ReturnCodes.SUCCESS;
            }

            _printer.SetFirearms(firearms.Value).Print(AnsiConsole.Console);
            AnsiConsole.Console.WriteSuccess("Firearms retrieved.");
            return ReturnCodes.SUCCESS;
        }
    }
}
