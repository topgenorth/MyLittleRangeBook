using ConsoleAppFramework;
using Fisher;
using Fisher.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Console;

namespace MyLittleRangeBook.Cartridges
{
    [RegisterCommands("cartridges")]
    public class CartridgeCommands : MlrbCommandBase
    {
        readonly IDocumentSession       _documentSession;
        readonly CartridgesTablePrinter _printer;
        readonly IQuerySession          _querySession;

        public CartridgeCommands(ILogger          logger,
                                 ICliDisplay      cliDisplay,
                                 IDocumentSession documentSession,
                                 IQuerySession    querySession) : base(logger, cliDisplay)
        {
            _documentSession = documentSession;
            _querySession    = querySession;
            _printer         = new CartridgesTablePrinter();
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
            CliDisplay.PrintCommandHeader("List cartridges");

            try
            {
                IReadOnlyList<Cartridge> x = await _querySession.Query<Cartridge>().ToListAsync(cancellationToken)
                                                                .ConfigureAwait(false);
                _printer.SetCartridges(x).Print(AnsiConsole.Console);

                CliDisplay.PrintSuccess("Cartridges retrieved.");

                return ReturnCodes.SUCCESS;
            }
            catch (Exception e)
            {
                CliDisplay.PrintFailure("Failed to retrieve cartridges: " + e.Message);
                return ReturnCodes.FAILURE;
            }
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
        public async Task<int> AddCartridge(string            name,
                                            string?           commonName        = null,
                                            double            diameterMetric    = 0,
                                            double            diameterImperial  = 0,
                                            bool              rifle             = false,
                                            bool              pistol            = false,
                                            CancellationToken cancellationToken = default)
        {
            CliDisplay.PrintCommandHeader("Add cartridge");
            Cartridge cartridge = new()
                                  {
                                      Name                       = name,
                                      CommonName                 = commonName,
                                      ProjectileDiameterMetric   = diameterMetric,
                                      ProjectileDiameterImperial = diameterImperial,
                                      SuitableForRifle           = rifle,
                                      SuitableForPistol          = pistol,
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