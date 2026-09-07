using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using MyLittleRangeBook.Console;
using MyLittleRangeBook.Firearms;

namespace MyLittleRangeBook
{
    [RegisterCommands("firearms")]
    [UsedImplicitly]
    public class AddReloadingRecipeToFirearmCommand
    {
        readonly ICliDisplay      _cliDisplay;
        readonly IFirearmsService _firearmsService;
        readonly ILogger          _logger;

        public AddReloadingRecipeToFirearmCommand(ICliDisplay cliDisplay, IFirearmsService firearmsService,
                                                  ILogger     logger)
        {
            _cliDisplay      = cliDisplay;
            _firearmsService = firearmsService;
            _logger          = logger;
        }

        /// <summary>
        ///     Adds a new reloading recipe to the specified firearm.
        /// </summary>
        /// <param name="firearm">The name of the firearm for which the recipe is being added.</param>
        /// <param name="cartridge">The name of the cartridge associated with the recipe.</param>
        /// <param name="description">The description of the ammunition recipe.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains an integer, where 0 indicates success (ReturnCodes.SUCCESS)
        ///     and 1 indicates failure (ReturnCodes.FAILURE).
        /// </returns>
        public async Task<int> AddRecipeToFirearm(string            firearm, string cartridge, string description,
                                                  CancellationToken cancellationToken = default)
        {
            _cliDisplay.PrintCommandHeader("Add Reloading Recipe to Firearm");
            if (string.IsNullOrWhiteSpace(firearm))
            {
                _logger.Warning("Firearm name is null or empty.");
                _cliDisplay.PrintFailure("Firearm name must be provided.");
                return ReturnCodes.FAILURE;
            }

            if (string.IsNullOrWhiteSpace(cartridge))
            {
                _logger.Warning("Cartridge name is null or empty.");
                _cliDisplay.PrintFailure("Cartridge name must be provided.");
                return ReturnCodes.FAILURE;
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                _logger.Warning("Ammo description is null or empty.");
                _cliDisplay.PrintFailure("Ammo description must be provided.");
                return ReturnCodes.FAILURE;
            }

            Result r = await _firearmsService.AddNewRecipe(firearm, cartridge, description, cancellationToken);
            if (r.IsFailed)
            {
                _logger.Error("Failed to fetch stream ID for firearm {0}.", firearm);
                _cliDisplay.PrintFailure("Failed to add reloading recipe to firearm.");
                return ReturnCodes.FAILURE;
            }

            _cliDisplay.PrintSuccess("Reloading recipe added successfully.");
            return ReturnCodes.SUCCESS;
        }
    }
}