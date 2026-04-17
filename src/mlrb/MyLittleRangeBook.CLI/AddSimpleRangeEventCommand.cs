using System.Data;
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
using static MyLittleRangeBook.CLI.ReturnCodes;
using static MyLittleRangeBook.Database.Sqlite.SqliteHelperExtensions;

namespace MyLittleRangeBook.CLI
{
    /// <summary>
    ///     Allows us to create a new Range Event from the CLI.
    /// </summary>
    [RegisterCommands("rangetrip")]
    [UsedImplicitly]
    public class AddSimpleRangeEventCommand
    {
        readonly ICliDisplay _cliDisplay;
        readonly ILogger _logger;
        readonly ISimpleRangeEventRepository _repo;
        readonly ISqliteHelper _sqliteHelper;

        public AddSimpleRangeEventCommand(ICliDisplay cliDisplay,
            ILogger logger,
            [FromKeyedServices(SQLITE_KEY)] ISimpleRangeEventRepository repo,
            ISqliteHelper sqliteHelper)
        {
            _cliDisplay = cliDisplay;
            _logger = logger;
            _repo = repo;
            _sqliteHelper = sqliteHelper;
        }


        /// <summary>
        ///     Add a new range trip.
        /// </summary>
        /// <param name="firearm"></param>
        /// <param name="rounds"></param>
        /// <param name="range"></param>
        /// <param name="ammo"></param>
        /// <param name="notes"></param>
        /// <param name="date">The date of the range trip in YYYY-MM-DD format. Default to today if omitted</param>
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
            CancellationToken cancellationToken = default)
        {
            _cliDisplay.WriteHeader("Add Range Trip");

            IAnsiConsole console = _cliDisplay.Console;

            await using SqliteConnection conn = await _sqliteHelper.GetDatabaseConnectionAsync(cancellationToken);

            (firearm, rounds) = await PromptForFirearmAndRounds(console, conn, firearm, rounds, cancellationToken);
            ammo = await PromptForAmmoNotes(console, conn, ammo, firearm, cancellationToken);
            notes = await PromptForTripNotes(console, notes, cancellationToken);
            range = await PromptForRangeSelection(console, conn, range, cancellationToken);

            try
            {
                await SaveToDatabase(firearm, rounds, range, ammo, notes, date, cancellationToken);

                return SUCCESS;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to add SimpleRangeEvent");

                return FAILED_TO_CREATE_RANGE_EVENT;
            }
        }

        static async Task<string> PromptForTripNotes(IAnsiConsole console,
            string notes,
            CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(notes))
            {
                return notes;
            }

            TextPrompt<string> p = new TextPrompt<string>("Enter any notes about the trip (optional)?")
                .AllowEmpty();
            notes = await console.PromptAsync(p, cancellationToken);

            return notes;
        }

        async Task<string> PromptForAmmoNotes(IAnsiConsole console,
            SqliteConnection conn,
            string firearm,
            string ammo,
            CancellationToken cancellationToken)
        {
            const string AMMO_SQL =
                "SELECT DISTINCT AmmoDescription FROM SimpleRangeEvents WHERE FirearmName=@firearmname ORDER BY AmmoDescription;";
            if (!string.IsNullOrWhiteSpace(ammo))
            {
                return ammo;
            }

            IPrompt<string> prompt;
            if (string.IsNullOrWhiteSpace(firearm))
            {
                prompt = new TextPrompt<string>("Enter [green]ammunition[/] (optional).")
                    .AllowEmpty();
            }
            else
            {
                var cmd = new SqliteCommand(AMMO_SQL, conn);
                cmd.Parameters.AddWithValue("@firearmname", firearm);
                prompt = new SelectionPrompt<string>()
                    .Title("Select [green]ammunition[/]")
                    .HighlightStyle(new Style(Color.Green, Color.Black, Decoration.Bold))
                    .EnableSearch()
                    .AddChoices(GetChoices(cmd));
            }

            ammo = await console.PromptAsync(prompt, cancellationToken);

            return ammo;
        }

        async Task<string> PromptForRangeSelection(IAnsiConsole console,
            SqliteConnection conn,
            string range,
            CancellationToken cancellationToken)
        {
            const string RANGE_SQL = "SELECT DISTINCT RangeName FROM SimpleRangeEvents ORDER BY RangeName;";
            if (!string.IsNullOrWhiteSpace(range))
            {
                return range;
            }

            SelectionPrompt<string> selectionPrompt = new SelectionPrompt<string>()
                .Title("Select a [green]range[/]")
                .HighlightStyle(new Style(Color.Green, Color.Black, Decoration.Bold))
                .EnableSearch()
                .AddChoices(GetChoices(conn, RANGE_SQL));

            range = await console.PromptAsync(selectionPrompt, cancellationToken);

            return range;
        }

        static IEnumerable<string> GetChoices(SqliteCommand command)
        {
            using SqliteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                yield return reader.GetString(0);
            }
        }

        static IEnumerable<string> GetChoices(SqliteConnection connection, string sql)
        {
            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandType = CommandType.Text;

            return GetChoices(command);
        }

        async Task<(string firearm, int rounds)> PromptForFirearmAndRounds(IAnsiConsole console,
            SqliteConnection conn,
            string firearm,
            int rounds,
            CancellationToken cancellationToken)
        {
            const string FIREARM_SQL = "SELECT DISTINCT FirearmName FROM SimpleRangeEvents ORDER BY FirearmName;";
            if (!string.IsNullOrWhiteSpace(firearm))
            {
                return (firearm, rounds);
            }

            SelectionPrompt<string> selectionPrompt = new SelectionPrompt<string>()
                .Title("Select a [green]firearm[/]")
                .HighlightStyle(new Style(Color.Green, Color.Black, Decoration.Bold))
                .EnableSearch()
                .AddChoices(GetChoices(conn, FIREARM_SQL));

            firearm = await console.PromptAsync(selectionPrompt, cancellationToken);

            if (rounds != 0)
            {
                return (firearm, rounds);
            }

            TextPrompt<int> p = new TextPrompt<int>("Enter the number of rounds fired (zero or greater)?")
                .DefaultValue(0)
                .Validate(x => x > 0);
            rounds = await console.PromptAsync(p, cancellationToken);

            return (firearm, rounds);
        }

        async Task SaveToDatabase(string firearm,
            int rounds,
            string range,
            string ammo,
            string notes,
            DateOnly date,
            CancellationToken cancellationToken)
        {
            var sre = new SimpleRangeEvent
            {
                FirearmName = firearm,
                RoundsFired = rounds,
                RangeName = range,
                AmmoDescription = ammo,
                Notes = notes,
                EventDate = date == default ? DateTime.Now.Date : date.ToDateTime(TimeOnly.MinValue).Date
            };

            // TODO [TO20260416] Data validation.
            Result<long?> result = await _repo.UpsertAsync(sre, cancellationToken);
        }
    }
}
