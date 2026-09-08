using System.Text.Json;
using ConsoleAppFramework;
using Fisher;
using JetBrains.Annotations;
using MyLittleRangeBook.Console;
using MyLittleRangeBook.Recipes;

namespace MyLittleRangeBook
{
    [RegisterCommands("recipes")]
    [UsedImplicitly]
    public class AddReloadingRecipeToFirearmCommand
    {
        readonly ICliDisplay      _cliDisplay;
        readonly ILogger          _logger;
        readonly IDocumentSession _session;

        public AddReloadingRecipeToFirearmCommand(ICliDisplay      cliDisplay,
                                                  ILogger          logger,
                                                  IDocumentSession session)
        {
            _cliDisplay = cliDisplay;
            _logger     = logger;
        }

        /// <summary>
        ///     Adds a new reloading recipe to the specified firearm.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains an integer, where 0 indicates success (ReturnCodes.SUCCESS)
        ///     and 1 indicates failure (ReturnCodes.FAILURE).
        /// </returns>
        [Command("add-json-file")]
        [UsedImplicitly]
        public async Task<int> AddRecipeJson(string            json,
                                             CancellationToken cancellationToken = default)
        {
            _cliDisplay.PrintCommandHeader("Add recipe");
            if (string.IsNullOrWhiteSpace(json))
            {
                _logger.Warning("JSON string is null or empty.");
                _cliDisplay.PrintFailure("JSON name must be provided.");
                return ReturnCodes.FAILURE;
            }

            Guid commandId = Guid.CreateVersion7();
            NewRecipeFromCommandLine e = new(
                                             Guid.CreateVersion7(),
                                             commandId,
                                             commandId,
                                             json,
                                             DateTimeOffset.UtcNow);
            ErrorDeserializingRecipeJson? e2 = null;


            Recipe? recipe;
            try
            {
                recipe               = JsonSerializer.Deserialize<Recipe>(json);
                recipe.CorrelationId = commandId;
                recipe.CausationId   = e.Id;
            }
            catch (Exception ex)
            {
                recipe = null;
                e2 = new ErrorDeserializingRecipeJson(
                                                 Guid.CreateVersion7(),
                                                 commandId,
                                                 e.Id,
                                                 json,
                                                 ex,
                                                 DateTimeOffset.UtcNow);

                _logger.Error(ex, "Failed to deserialize JSON to Recipe.");
                _cliDisplay.PrintFailure("Invalid JSON format.");
                return ReturnCodes.FAILURE;
            }

            _session.Store(recipe);
            await _session.SaveChangesAsync(cancellationToken);
            int returnCode = ReturnCodes.SUCCESS;

            if (recipe == null)
            {
                _logger.Warning("Deserialized recipe is null.");
                _cliDisplay.PrintFailure("Failed to deserialize recipe.");
                returnCode =  ReturnCodes.FAILURE;
            }
            else
            {
                _cliDisplay.PrintSuccess("Reloading recipe added successfully.");
            }

            return returnCode;
        }
    }
}