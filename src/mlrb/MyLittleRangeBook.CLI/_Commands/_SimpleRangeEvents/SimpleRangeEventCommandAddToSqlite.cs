using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using MyLittleRangeBook.Services;
using static MyLittleRangeBook.CLI.ReturnCodes;
using static MyLittleRangeBook.Database.Sqlite.SqliteHelperExtensions;

namespace MyLittleRangeBook.CLI
{
    /// <summary>
    ///     Allows us to create a new Range Event from the CLI, and optionally the FIT file that goes with it.
    /// </summary>
    [RegisterCommands("rangeevent")]
    [UsedImplicitly]
    public class SimpleRangeEventCommandAddToSqlite: MlrbCommandBase
    {
        readonly ISimpleRangeEventHelper _rangeEventHelper;
        readonly ISimpleRangeEventRepository _repo;
        readonly ISimpleRangeEventPrinter _simpleRangeEventPrinter;

        public SimpleRangeEventCommandAddToSqlite(ILogger logger,
            ICliDisplay cliDisplay,
            [FromKeyedServices(DI_KEYS_SQLITE)] ISimpleRangeEventRepository repo,
            [FromKeyedServices(DI_KEYS_SQLITE)] ISimpleRangeEventHelper rangeEventHelper,
            ISimpleRangeEventPrinter simpleRangeEventPrinter): base(logger, cliDisplay)
        {
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
        /// <param name="eventDate">The eventDate of the range trip in YYYY-MM-DD format. Default to today if omitted.</param>
        /// <param name="quiet">If this parameter is provided, then the command will display minimal output to the console.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Command("add")]
        [UsedImplicitly]
        public async Task<int> AddSimpleRangeEventAsync(string firearm = "",
            int rounds = 0,
            string range = "",
            string ammo = "",
            string notes = "",
            [RangeTripDateParser] DateOnly? eventDate = null,
            bool quiet = false,
            CancellationToken cancellationToken = default)
        {
            CliDisplay.PrintCommandHeader("Add range event");
            Result<(List<string>, List<string>)> r1 = await _rangeEventHelper
                .GetFirearmsAndRangesAsync(cancellationToken)
                .ConfigureAwait(false);
            if (r1.IsFailed)
            {
                Logger.Warning("Failed to retrieve list of firearms and/or ranges.");
            }

            List<string> firearms = r1.Value.Item1;
            List<string> ranges = r1.Value.Item2;

            DateOnly dateOnly;
            if (eventDate is null)
            {
                DateTime d = DateTime.Now;
                dateOnly = DateOnly.FromDateTime(d);
            }
            else
            {
                dateOnly = eventDate.Value;
            }

            try
            {
                SimpleRangeEvent sre = await AskUserForMissingDataOnSimpleRangeEventAsync(firearm, rounds, range, ammo, notes, dateOnly,
                        firearms, ranges, cancellationToken)
                    .ConfigureAwait(false);

                if (cancellationToken.IsCancellationRequested)
                {
                    Logger.Warning("Operation cancelled by user.");
                    CliDisplay.PrintFailure("Operation cancelled.");

                    return COMMAND_CANCELLED;
                }

                Result<long?> result = await _repo.UpsertAsync(sre, cancellationToken).ConfigureAwait(false);

                if (result.IsSuccess)
                {

                    _simpleRangeEventPrinter.Print(CliDisplay.Console, sre, quiet);
                    CliDisplay.PrintSuccess("Range trip added successfully.");
                    return SUCCESS;
                }

                CliDisplay.PrintFailure("Failed to add range trip.");

                return RANGE_EVENT_FAILED_TO_CREATE;
            }
            catch (TaskCanceledException tce)
            {
                Logger.Warning(tce, "AddSimpleRangeEventAsync was cancelled.s");
                CliDisplay.PrintFailure("AddSimpleRangeEventAsync was cancelled.");

                return COMMAND_CANCELLED;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Unexpected error trying to add SimpleRangeEvent");
                CliDisplay.PrintFailure($"Unexpected error trying to add SimpleRangeEvent: {e.Message}");

                return RANGE_EVENT_FAILED_TO_CREATE;
            }
        }


        async Task<SimpleRangeEvent> AskUserForMissingDataOnSimpleRangeEventAsync(string firearm,
            int rounds,
            string range,
            string ammo,
            string notes,
            DateOnly date,
            List<string> firearms,
            List<string> ranges,
            CancellationToken cancellationToken)
        {
            firearm = await AskUserForFirearmAsync(firearm, firearms, cancellationToken).ConfigureAwait(false);
            rounds = await AskUserForRoundCountAsync(rounds, cancellationToken).ConfigureAwait(false);
            range = await AskUserForRangeAsync(range, ranges, cancellationToken).ConfigureAwait(false);
            ammo = await AskUserForAmmoAsync(firearm, ammo, cancellationToken).ConfigureAwait(false);
            notes = await AskUserForNotesAsync(notes, cancellationToken).ConfigureAwait(false);
            var sre = SimpleRangeEvent.New(
                RemoveSurroundingQuotes(firearm),
                rounds,
                RemoveSurroundingQuotes(range),
                RemoveSurroundingQuotes(ammo),
                RemoveSurroundingQuotes(notes),
                date);

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
            notes = await CliDisplay.Console.PromptAsync(p, cancellationToken).ConfigureAwait(false);

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

            return await CliDisplay.Console.PromptAsync(prompt, cancellationToken).ConfigureAwait(false);
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

            return await CliDisplay.Console.PromptAsync(prompt, cancellationToken).ConfigureAwait(false);
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

            firearm = await CliDisplay.Console.PromptAsync(prompt, cancellationToken).ConfigureAwait(false);

            return firearm;
        }

        async Task<int> AskUserForRoundCountAsync(int roundCount, CancellationToken cancellationToken)
        {
            if (roundCount > 0)
            {
                return roundCount;
            }

            TextPrompt<int> p = new TextPrompt<int>("      [green]Rounds[/]")
                .DefaultValue(0)
                .Validate(x => x > 0);
            roundCount = await CliDisplay.Console.PromptAsync(p, cancellationToken).ConfigureAwait(false);

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
