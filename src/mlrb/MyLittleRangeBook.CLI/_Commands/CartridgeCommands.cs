using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using MyLittleRangeBook.CLI.Console;
using MyLittleRangeBook.Database.Sqlite;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Services;
using NanoidDotNet;
using Spectre.Console;
namespace MyLittleRangeBook.CLI
{
    [RegisterCommands("cartridge")]
    public class CartridgeCommands: MlrbCommandBase
    {
        readonly ICartridgesDbService _cartridgesDbService;
        readonly CartridgesTablePrinter _printer;
        readonly ISqliteHelper _sqliteHelper;
        public CartridgeCommands(ILogger logger, ICliDisplay cliDisplay,
            [FromKeyedServices(SqliteHelperExtensions.DI_KEYS_SQLITE)] ICartridgesDbService cartridgesDbService,
            ISqliteHelper sqliteHelper): base (logger, cliDisplay)
        {
            _cartridgesDbService = cartridgesDbService;
            _sqliteHelper = sqliteHelper;
            _printer = new CartridgesTablePrinter();
        }
        /// <summary>
        ///     List all the active cartridges.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Command("list")]
        [UsedImplicitly]
        public async Task<int> PrintCartridgesToConsole(CancellationToken cancellationToken = default)
        {
            AnsiConsole.Console.PrintAppInfo();
            AnsiConsole.Console.WriteLine("Retrieving cartridges...");
            await using SqliteConnection conn = await _sqliteHelper
                .GetDatabaseConnectionAsync(cancellationToken)
                .ConfigureAwait(false);
            Result<IEnumerable<Cartridge>> cartridges = await _cartridgesDbService
                .GetCartridgesAsync(conn, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            if (cartridges.IsFailed)
            {
                Logger.Warning("Failed to retrieve cartridges.");
                AnsiConsole.Console.PrintProblem("Failed to retrieve cartridges.");
                return ReturnCodes.FAILURE;
            }
            if (!cartridges.Value.Any())
            {
                AnsiConsole.Console.WriteWarning("No cartridges found.");
                return ReturnCodes.SUCCESS;
            }
            _printer.SetCartridges(cartridges.Value).Print(AnsiConsole.Console);
            AnsiConsole.Console.PrintSuccess("Cartridges retrieved.");
            return ReturnCodes.SUCCESS;
        }
        /// <summary>
        ///     Add a new cartridge.
        /// </summary>
        /// <param name="name">The name of the cartridge.</param>
        /// <param name="commonName">The common name of the cartridge.</param>
        /// <param name="diameterMetric">Projectile diameter in mm.</param>
        /// <param name="diameterImperial">Projectile diameter in inches.</param>
        /// <param name="rifle">Suitable for rifles (true/false).</param>
        /// <param name="pistol">Suitable for pistols (true/false).</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Command("add")]
        [UsedImplicitly]
        public async Task<int> AddCartridge(string name, string? commonName = null, double diameterMetric = 0, double diameterImperial = 0, bool rifle = false, bool pistol = false, CancellationToken cancellationToken = default)
        {
            AnsiConsole.Console.PrintAppInfo();
            AnsiConsole.Console.WriteLine("Adding cartridge...");
            var cartridge = new Cartridge
            {
                Id = await Nanoid.GenerateAsync().ConfigureAwait(false),
                Name = name,
                CommonName = commonName,
                ProjectileDiameterMetric = diameterMetric,
                ProjectileDiameterImperial = diameterImperial,
                SuitableForRifle = rifle,
                SuitableForPistol = pistol
            };
            await using SqliteConnection conn = await _sqliteHelper
                .GetDatabaseConnectionAsync(cancellationToken)
                .ConfigureAwait(false);
            Result<EntityId> result = await _cartridgesDbService
                .UpsertAsync(conn, cartridge, cancellationToken)
                .ConfigureAwait(false);
            if (result.IsFailed)
            {
                Logger.Warning("Failed to add cartridge.");
                AnsiConsole.Console.PrintProblem("Failed to add cartridge.");
                return ReturnCodes.FAILURE;
            }
            AnsiConsole.Console.PrintSuccess($"Cartridge '{name}' added with ID {result.Value.Id}.");
            return ReturnCodes.SUCCESS;
        }
    }
}
