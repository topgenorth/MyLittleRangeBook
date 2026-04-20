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
    public class AddSimpleRangeEventToSqliteCommand
    {
        const string RangeSql = "SELECT DISTINCT RangeName FROM SimpleRangeEvents ORDER BY Modified DESC,  RangeName;";

        const string FirearmSql =
            "SELECT DISTINCT FirearmName FROM SimpleRangeEvents ORDER BY Modified DESC,  FirearmName;";

        const string AmmoSql =
            "SELECT DISTINCT AmmoDescription FROM SimpleRangeEvents WHERE FirearmName=@firearmname ORDER BY Modified DESC, AmmoDescription;";

        readonly ICliDisplay _cliDisplay;
        readonly ILogger _logger;
        readonly ISimpleRangeEventRepository _repo;
        readonly ISqliteHelper _sqliteHelper;

        public AddSimpleRangeEventToSqliteCommand(ICliDisplay cliDisplay,
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
            Result<bool> migrations = await _sqliteHelper.ApplyDbupMigrationsAsync(cancellationToken);

            IAnsiConsole console = _cliDisplay.Console;

            IEnumerable<string> ammoChoices = [];

            try
            {
                await using SqliteConnection conn = await _sqliteHelper.GetDatabaseConnectionAsync(cancellationToken);
                IEnumerable<string> firearmChoices = GetChoices(conn, FirearmSql);
                IEnumerable<string> rangeChoices = GetChoices(conn, RangeSql);

                (firearm, rounds) =
                    await PromptForFirearmAndRounds(console, firearm, firearmChoices, rounds, cancellationToken);
                if (!string.IsNullOrWhiteSpace(firearm))
                {
                    var cmd = new SqliteCommand(AmmoSql, conn);
                    cmd.Parameters.AddWithValue("@firearmname", firearm);
                    ammoChoices = GetChoices(cmd);
                }

                range = await PromptForRangeSelection(console, range, rangeChoices, cancellationToken);
                ammo = await PromptForAmmoNotes(console, ammo, ammoChoices, cancellationToken);
                notes = await PromptForTripNotes(console, notes, cancellationToken);

                SimpleRangeEvent sre =
                    await SaveToDatabaseAsync(firearm, rounds, range, ammo, notes, date, cancellationToken);

                DisplayToConsole(console, sre);

                return SUCCESS;
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to add SimpleRangeEvent");
                _cliDisplay.WriteFailure($"Failed to add SimpleRangeEvent: {e.Message}");

                return FAILED_TO_CREATE_RANGE_EVENT;
            }
        }

        void DisplayToConsole(IAnsiConsole console, SimpleRangeEvent sre)
        {
            Grid bodyGrid = new Grid().AddColumns(2);
            bodyGrid.AddRow("", "[green]Range Trip Added[/]");

            bodyGrid.AddRow("  [white]RowId:[/]", sre.RowId.ToString() ?? string.Empty);
            bodyGrid.AddRow("  [white]Id:[/]", sre.Id);
            bodyGrid.AddRow("  [white]Date:[/]", sre.EventDate.ToString("yyyy-MMM-dd"));
            bodyGrid.AddRow("  [white]Firearm:[/]", sre.FirearmName);
            bodyGrid.AddRow("  [white]Range:[/] ", sre.RangeName);

            if (sre.RoundsFired > 0)
            {
                bodyGrid.AddRow("  [white]Rounds:[/] ", sre.RoundsFired.ToString());
            }

            if (!string.IsNullOrWhiteSpace(sre.AmmoDescription))
            {
                bodyGrid.AddRow("  [white]Ammo:[/] ", sre.AmmoDescription);
            }

            if (!string.IsNullOrWhiteSpace(sre.Notes))
            {
                bodyGrid.AddRow("  [white]Notes:[/] ", sre.Notes);
            }

            Layout layout = new Layout("root")
                .SplitRows(new Layout("header"), new Layout("body"));
            Grid headerGrid = CreateHeaderGrid();
            layout["header"].Update(headerGrid);
            layout["body"].Update(bodyGrid);
            console.Write(layout);
        }

        Grid CreateHeaderGrid()
        {
            var headerGrid = new Grid();
            headerGrid.AddColumn();
            headerGrid.AddRow($"[bold]{Markup.Escape(CliDisplay.AppName)}[/]");
            headerGrid.AddRow($"[grey]Version:[/] [green]{Markup.Escape(_cliDisplay.AppVersion)}[/]");
            return headerGrid;
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

            return await console.PromptAsync(prompt, cancellationToken);
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

            firearm = await console.PromptAsync(prompt, cancellationToken);

            if (rounds != 0)
            {
                return (firearm, rounds);
            }

            TextPrompt<int> p = new TextPrompt<int>("      [green]Rounds[/]")
                .DefaultValue(0)
                .Validate(x => x > 0);
            rounds = await console.PromptAsync(p, cancellationToken);

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

            sre.RowId = result.Value ?? -1;

            return sre;
        }
    }
}
