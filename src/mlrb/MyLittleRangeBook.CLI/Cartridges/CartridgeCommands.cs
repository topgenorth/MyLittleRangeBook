using ConsoleAppFramework;
using Fisher;
using FluentResults;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using MyLittleRangeBook.Console;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook.Cartridges
{
    [RegisterCommands("cartridges")]
    public class CartridgeCommands : MlrbCommandBase
    {
        readonly ICartridgesService _cartridgesService;
        readonly CartridgesTablePrinter _printer;
        readonly ISqliteHelper _sqliteHelper;
        readonly IDocumentSession _documentSession;

        public CartridgeCommands(ILogger                                          logger,
            ICliDisplay                                                           cliDisplay,
            [FromKeyedServices(SqliteHelperExtensions.DI_KEY)] ICartridgesService cartridgesService,
            ISqliteHelper                                                         sqliteHelper,
            IDocumentSession documentSession) : base(logger, cliDisplay)
        {
            _cartridgesService    = cartridgesService;
            _sqliteHelper         = sqliteHelper;
            _documentSession = documentSession;
            _printer              = new CartridgesTablePrinter();
        }

        /// <summary>
        ///     List all the active cartridges.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Command("list"), UsedImplicitly]
        public async Task<int> PrintCartridgesToConsole(CancellationToken cancellationToken = default)
        {
            CliDisplay.PrintCommandHeader("List cartridges");

            await using ScopedSqliteConnection scoped =
                await _sqliteHelper.GetScopedDatabaseConnectionAsync(cancellationToken).ConfigureAwait(false);
            await using SqliteConnection conn = scoped.Connection;
            Result<IEnumerable<Cartridge>> cartridges = await _cartridgesService
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
                AnsiConsole.Console.PrintWarning("No cartridges found.");

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
        [Command("add"), UsedImplicitly]
        public async Task<int> AddCartridge(string name,
            string? commonName = null,
            double diameterMetric = 0,
            double diameterImperial = 0,
            bool rifle = false,
            bool pistol = false,
            CancellationToken cancellationToken = default)
        {
            CliDisplay.PrintCommandHeader("Add cartridge");
            var cartridge = new Cartridge
            {
                Name = name,
                CommonName = commonName,
                ProjectileDiameterMetric = diameterMetric,
                ProjectileDiameterImperial = diameterImperial,
                SuitableForRifle = rifle,
                SuitableForPistol = pistol
            };


            _documentSession.Store(cartridge);

            try
            {
                await _documentSession.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (SqliteException ex)
            {
                CliDisplay.PrintFailure($"Failed to add cartridge: {ex.Message}");
                return ReturnCodes.FAILURE;
            }

            CliDisplay.PrintSuccess($"Cartridge '{name}' added with ID {cartridge.Id}.");

            return ReturnCodes.SUCCESS;
        }
    }
}
