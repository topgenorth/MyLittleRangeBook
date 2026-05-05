using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using MyLittleRangeBook.CLI.Database.Sqlite;
using MyLittleRangeBook.Database.Sqlite;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Services;
using Spectre.Console;
using static MyLittleRangeBook.CLI.ReturnCodes;
using static MyLittleRangeBook.Database.Sqlite.SqliteHelperExtensions;

namespace MyLittleRangeBook.CLI.Console
{
    /// <summary>
    ///     Allows us to create a new Range Event from the CLI.
    /// </summary>
    [RegisterCommands("rangetrip")]
    [UsedImplicitly]
    public class AddSimpleRangeEventToSqliteCommand
    {
        readonly ICliDisplay _cliDisplay;
        readonly ILogger _logger;
        readonly ISimpleRangeEventHelper _rangeEventHelper;
        readonly ISimpleRangeEventRepository _repo;
        readonly ISimpleRangeEventPrinter _simpleRangeEventPrinter;

        public AddSimpleRangeEventToSqliteCommand(ICliDisplay cliDisplay,
            ILogger logger,
            [FromKeyedServices(DI_KEYS_SQLITE)] ISimpleRangeEventRepository repo,
            [FromKeyedServices(DI_KEYS_SQLITE)] ISimpleRangeEventHelper rangeEventHelper,
            ISimpleRangeEventPrinter simpleRangeEventPrinter)
        {
            _cliDisplay = cliDisplay;
            _logger = logger;
            _repo = repo;
            _rangeEventHelper = rangeEventHelper;
            _simpleRangeEventPrinter = simpleRangeEventPrinter;
        }

        /// <summary>
        ///     Add a new range trip.
        /// </summary>
        /// <param name="firearm">
        ///     The name of the firearm. If this is omitted, then the CLI will promot for values based on what is
        ///     in the database already.
        /// </param>
        /// <param name="rounds">How many rounds were used. Required. Must be zero or greater.</param>
        /// <param name="range">The name of the shooting range.</param>
        /// <param name="ammo">A description of the ammo used. The recommended format is PROJECTILE[,|;]POWDER[</param>
        /// <param name="notes">Any notes or comments.  Optional</param>
        /// <param name="date">The date of the range trip in YYYY-MM-DD format. Default to today if omitted</param>
        /// <param name="fitFile">The path to a Garmin FIT file from the Xero C1.</param>
        /// <param name="quiet">If this parameter is provided, then the command will display minimal output the the console.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Command("add")]
        [UsedImplicitly]
        public async Task<int> AddSimpleRangeEventAsync(string firearm = "",
            int rounds = 0,
            string range = "",
            string ammo = "",
            string notes = "",
            [RangeTripDateParser] DateOnly date = default,
            string fitFile = "",
            bool quiet = false,
            CancellationToken cancellationToken = default)
        {
            Result<(List<string>, List<string>)> r1 =
                await _rangeEventHelper.GetFirearmsAndRangesAsync(cancellationToken).ConfigureAwait(false);
            if (r1.IsFailed)
            {
                _logger.Warning("Failed to retrieve list of firearms and/or ranges.");
            }

            List<string> firearms = r1.Value.Item1;
            List<string> ranges = r1.Value.Item2;

            try
            {
                SimpleRangeEvent sre = await CreateSimpleRangeEvent(firearm, rounds, range, ammo, notes, date, cancellationToken, firearms, ranges).ConfigureAwait(true);
                _cliDisplay.WriteSuccess("Range trip added successfully.");
                if (!string.IsNullOrWhiteSpace(fitFile))
                {
                    Result fitResult = await ProcessFitFileAsync(fitFile, sre, cancellationToken)
                        .ConfigureAwait(false);
                    if (fitResult.IsFailed)
                    {
                        _cliDisplay.WriteFailure($"Failed to process FIT file: {fitFile}");
                    }
                }


                _simpleRangeEventPrinter.PrintToConsole(_cliDisplay.Console, sre, quiet);

                return SUCCESS;
            }
            catch (TaskCanceledException tce)
            {
                _logger.Warning(tce, "AddSimpleRangeEventAsync was cancelled.s");
                _cliDisplay.WriteFailure("AddSimpleRangeEventAsync was cancelled.");

                return COMMAND_CANCELLED;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unexpected error trying to add SimpleRangeEvent");
                _cliDisplay.WriteFailure($"Unexpected error trying to add SimpleRangeEvent: {e.Message}");

                return RANGE_EVENT_FAILED_TO_CREATE;
            }
        }

        async Task<Result> ProcessFitFileAsync(string fitFile, SimpleRangeEvent sre, CancellationToken cancellationToken)
        {
            return await Task.FromResult(Result.Fail("Not yet implemented.")).ConfigureAwait(false);
        }

        async Task<SimpleRangeEvent> CreateSimpleRangeEvent(string firearm,
            int rounds,
            string range,
            string ammo,
            string notes,
            DateOnly date,
            CancellationToken cancellationToken,
            List<string> firearms,
            List<string> ranges)
        {
            firearm = await AskUserForFirearmAsync(firearm, firearms, cancellationToken).ConfigureAwait(true);
            rounds = await AskUserForRoundCountAsync(rounds, cancellationToken).ConfigureAwait(true);
            range = await AskUserForRangeAsync(range, ranges, cancellationToken).ConfigureAwait(true);
            ammo = await AskUserForAmmoAsync(firearm, ammo, cancellationToken).ConfigureAwait(true);
            notes = await AskUserForNotesAsync(notes, cancellationToken).ConfigureAwait(true);

            var sre = SimpleRangeEvent.New(
                RemoveSurroundingQuotes(firearm),
                rounds,
                RemoveSurroundingQuotes(range),
                RemoveSurroundingQuotes(ammo),
                RemoveSurroundingQuotes(notes),
                date);

            // TODO [TO20260416] Data validation.
            Result<long?> result = await _repo.UpsertAsync(sre, cancellationToken).ConfigureAwait(false);

            sre.RowId = result.Value ?? -1;

            return sre;
        }

        async Task<string> AskUserForNotesAsync(string notes, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(notes))
            {
                return notes;
            }

            TextPrompt<string> p = new TextPrompt<string>("Enter any [green]notes[/] (optional)")
                .AllowEmpty();
            notes = await _cliDisplay.Console.PromptAsync(p, cancellationToken).ConfigureAwait(true);

            return notes;
        }

        async Task<string> AskUserForAmmoAsync(string firearm, string ammo, CancellationToken cancellationToken)
        {
            // [TO20260503] We should probably only prompt for ammo if we have a firearm, and we should use the firearm to filter the ammo choices.
            if (!string.IsNullOrWhiteSpace(ammo) || string.IsNullOrWhiteSpace(firearm))
            {
                return ammo;
            }

            Result<List<string>> ammoChoices = await _rangeEventHelper
                .GetAmmoDescriptionsForFirearmAsync(firearm, cancellationToken)
                .ConfigureAwait(false);

            IPrompt<string> prompt;

            if (ammoChoices.IsFailed || ammoChoices.Value.Count == 0)
            {
                prompt = new TextPrompt<string>("Enter [green]ammunition[/] (optional)?");
            }
            else
            {
                prompt = new SelectionPrompt<string>()
                    .Title("Select [green]ammunition[/]")
                    .HighlightStyle(new Style(Color.Green, Color.Black, Decoration.Bold))
                    .EnableSearch()
                    .AddChoices(ammoChoices.Value);
            }

            return await _cliDisplay.Console.PromptAsync(prompt, cancellationToken).ConfigureAwait(true);
        }

        async Task<string> AskUserForRangeAsync(string range,
            IEnumerable<string> rangeChoices,
            CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(range))
            {
                return range;
            }

            IPrompt<string> prompt;
            IEnumerable<string> enumerable = rangeChoices as string[] ?? rangeChoices.ToArray();
            if (enumerable.Any())
            {
                prompt = new SelectionPrompt<string>()
                    .Title("Select a [green]range[/]")
                    .HighlightStyle(new Style(Color.Green, Color.Black, Decoration.Bold))
                    .EnableSearch()
                    .AddChoices(enumerable);
            }
            else
            {
                prompt = new TextPrompt<string>("Enter [green]range[/]?");
            }

            return await _cliDisplay.Console.PromptAsync(prompt, cancellationToken).ConfigureAwait(true);
        }


        async Task<string> AskUserForFirearmAsync(string firearm,
            IEnumerable<string> firearms,
            CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(firearm))
            {
                return firearm;
            }

            IPrompt<string> prompt;
            IEnumerable<string> choices = firearms as string[] ?? firearms.ToArray();
            if (choices.Any())
            {
                prompt = new SelectionPrompt<string>()
                    .Title("Select a [green]firearm[/]")
                    .HighlightStyle(new Style(Color.Green, Color.Black, Decoration.Bold))
                    .EnableSearch()
                    .AddChoices(choices);
            }
            else
            {
                prompt = new TextPrompt<string>("Enter [green]firearm[/]?");
            }

            firearm = await _cliDisplay.Console.PromptAsync(prompt, cancellationToken).ConfigureAwait(true);

            return firearm;
        }

        async Task<int> AskUserForRoundCountAsync(int roundCount, CancellationToken cancellationToken)
        {
            if (roundCount < 1)
            {
                return roundCount;
            }

            TextPrompt<int> p = new TextPrompt<int>("      [green]Rounds[/]")
                .DefaultValue(0)
                .Validate(x => x > 0);
            roundCount = await _cliDisplay.Console.PromptAsync(p, cancellationToken).ConfigureAwait(true);

            return roundCount;
        }


        /// <summary>
        ///     Will strip the double quotes from the start and end of to the string.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        static string RemoveSurroundingQuotes(string value)
        {
            return value.Length >= 2 && value.StartsWith('"') && value.EndsWith('"')
                ? value[1..^1]
                : value;
        }
    }
}
