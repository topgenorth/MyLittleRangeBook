using System.Data.Common;
using ConsoleAppFramework;
using Dapper;
using FluentResults;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using MyLittleRangeBook.Console;
using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook
{
    [RegisterCommands("firearms")]
    public class FirearmCommands : MlrbCommandBase
    {



        readonly IFirearmAggregateRepository _repo;
        readonly IFirearmsService _firearmsService;
        readonly FirearmsTablePrinter _printer;
        readonly ISqliteHelper _sqliteHelper;

        public FirearmCommands(ILogger logger,
            ICliDisplay cliDisplay,
            [FromKeyedServices(SqliteHelperExtensions.DI_KEYS_SQLITE)] IFirearmsService firearmsService,
            ISqliteHelper sqliteHelper,
            IFirearmAggregateRepository repo) : base(logger, cliDisplay)
        {
            _firearmsService = firearmsService;
            _sqliteHelper = sqliteHelper;
            _repo = repo;
            _printer = new FirearmsTablePrinter();
        }



        /// <summary>
        ///     List all the active firearms.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Command("list"), UsedImplicitly]
        public async Task<int> PrintFirearmsToConsole(CancellationToken cancellationToken = default)
        {
            CliDisplay.PrintCommandHeader("List firearms");

            await using ScopedSqliteConnection scopedConn = await _sqliteHelper
                .GetScopedDatabaseConnectionAsync(cancellationToken)
                .ConfigureAwait(false);
            var ctx = new DapperCommandContext(scopedConn.Connection, null, cancellationToken);

            Result<IEnumerable<Firearm>> firearms = await _firearmsService
                .GetFirearmsAsync(ctx)
                .ConfigureAwait(false);

            if (firearms.IsFailed)
            {
                Logger.Warning("Failed to retrieve firearms.");
                CliDisplay.PrintFailure("Failed to retrieve list of firearms.");

                return ReturnCodes.FAILURE;
            }

            if (!firearms.Value.Any())
            {
                CliDisplay.PrintWarning("No firearms to list.");

                return ReturnCodes.SUCCESS;
            }

            _printer.SetFirearms(firearms.Value).Print(AnsiConsole.Console);
            CliDisplay.PrintSuccess("Firearms listed.");

            return ReturnCodes.SUCCESS;
        }
    }
}
