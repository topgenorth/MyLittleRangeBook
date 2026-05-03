using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using MyLittleRangeBook.CLI.Console;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Services;
using Spectre.Console;
using static MyLittleRangeBook.CLI.ReturnCodes;
using static MyLittleRangeBook.Database.Sqlite.SqliteHelperExtensions;

namespace MyLittleRangeBook.CLI
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
            // if (!quiet)
            // {
            //     Result<bool> migrations = await _sqliteHelper.ApplyDbupMigrationsAsync(cancellationToken);
            // }

            IAnsiConsole console = _cliDisplay.Console;

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
                (firearm, rounds) =
                    await PromptForFirearmAndRounds(console, firearm, rounds, cancellationToken);


                range = await PromptForRangeSelection(console, range, cancellationToken);
                ammo = await PromptForAmmoNotes(console, ammo, cancellationToken);
                notes = await PromptForTripNotes(console, notes, cancellationToken);

                SimpleRangeEvent sre =
                    await SaveToDatabaseAsync(firearm, rounds, range, ammo, notes, date, cancellationToken).ConfigureAwait(false);

                _simpleRangeEventPrinter.PrintToConsole(console, sre, quiet);

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


        async Task<string> PromptForTripNotes(IAnsiConsole console,
            string notes,
            CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(notes))
            {
                return notes;
            }

            TextPrompt<string> p = new TextPrompt<string>("Enter any [green]notes[/] (optional)")
                .AllowEmpty();
            notes = await console.PromptAsync(p, cancellationToken);

            return notes;
        }

        async Task<string> PromptForAmmoNotes(IAnsiConsole console,
            string ammo,
            IEnumerable<string> ammoChoices,
            CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(ammo))
            {
                return ammo;
            }

            IPrompt<string> prompt;
            if (ammoChoices.Any())
            {
                prompt = new SelectionPrompt<string>()
                    .Title("Select [green]ammunition[/]")
                    .HighlightStyle(new Style(Color.Green, Color.Black, Decoration.Bold))
                    .EnableSearch()
                    .AddChoices(ammoChoices);
            }
            else
            {
                prompt = new TextPrompt<string>("Enter [green]ammunition[/] (optional)?");
            }

            return await console.PromptAsync(prompt, cancellationToken);
        }

        async Task<string> PromptForRangeSelection(IAnsiConsole console,
            string range,
            IEnumerable<string> rangeChoices,
            CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(range))
            {
                return range;
            }

            IPrompt<string> prompt;
            if (rangeChoices.Any())
            {
                prompt = new SelectionPrompt<string>()
                    .Title("Select a [green]range[/]")
                    .HighlightStyle(new Style(Color.Green, Color.Black, Decoration.Bold))
                    .EnableSearch()
                    .AddChoices(rangeChoices);
            }
            else
            {
                prompt = new TextPrompt<string>("Enter [green]range[/]?");
            }

            return await console.PromptAsync(prompt, cancellationToken).ConfigureAwait(true);
        }


        async Task<(string firearm, int rounds)> PromptForFirearmAndRounds(IAnsiConsole console,
            string firearm,
            IEnumerable<string> firearmChoices,
            int rounds,
            CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(firearm))
            {
                return (firearm, rounds);
            }

            IPrompt<string> prompt;

            if (firearmChoices.Any())
            {
                prompt = new SelectionPrompt<string>()
                    .Title("Select a [green]firearm[/]")
                    .HighlightStyle(new Style(Color.Green, Color.Black, Decoration.Bold))
                    .EnableSearch()
                    .AddChoices(firearmChoices);
            }
            else
            {
                prompt = new TextPrompt<string>("Enter [green]firearm[/]?");
            }

            firearm = await console.PromptAsync(prompt, cancellationToken).ConfigureAwait(true);

            if (rounds != 0)
            {
                return (firearm, rounds);
            }

            TextPrompt<int> p = new TextPrompt<int>("      [green]Rounds[/]")
                .DefaultValue(0)
                .Validate(x => x > 0);
            rounds = await console.PromptAsync(p, cancellationToken).ConfigureAwait(true);

            return (firearm, rounds);
        }

        async Task<SimpleRangeEvent> SaveToDatabaseAsync(string firearm,
            int rounds,
            string range,
            string ammo,
            string notes,
            DateOnly date,
            CancellationToken cancellationToken)
        {
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
