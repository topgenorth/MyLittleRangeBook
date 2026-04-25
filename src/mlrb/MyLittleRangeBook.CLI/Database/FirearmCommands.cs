using ConsoleAppFramework;
using FluentResults;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using MyLittleRangeBook.CLI.Console;
using MyLittleRangeBook.Database.Sqlite;
using MyLittleRangeBook.Models;
using MyLittleRangeBook.Services;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace MyLittleRangeBook.CLI.Database
{

    class FirearmsTablePrinter : IConsolePrinter
    {
        IEnumerable<Firearm> _firearms = [];

        public FirearmsTablePrinter SetFirearms(IEnumerable<Firearm> firearms)
        {
            _firearms = firearms;

            return this;
        }
        public void Print(IAnsiConsole console)
        {
            console.Write(BuildRenderable());
        }

        public IRenderable BuildRenderable()
        {
            Table table = new Table()
                .Border(TableBorder.Rounded)
                .Expand()
                .BorderColor(Color.White)
                .AddColumn("Name", col => col.Alignment(Justify.Left))
                .AddColumn("Notes", col => col.Alignment(Justify.Left))
                .AddColumn("Id", col => col.Alignment(Justify.Center).Width(21))
                .AddColumn("Row Id", col => col.Alignment(Justify.Center).Width(6));

            foreach (Firearm firearm in _firearms)
            {
                table.AddRow(firearm.Name, firearm.Notes ?? string.Empty, firearm.Id!, firearm.RowId!.ToString() ?? "");
            }

            Panel p = new Panel(table).Expand().Border(BoxBorder.None);

            return p;
        }
    }
    [RegisterCommands("firearm")]
    public class FirearmCommands
    {
        readonly ILogger _logger;
        readonly ISqliteHelper _sqliteHelper;
        readonly IFirearmsService _firearmsService;
        readonly FirearmsTablePrinter _printer;

        public FirearmCommands([FromKeyedServices(SqliteHelperExtensions.SQLITE_KEY)]IFirearmsService firearmsService, ISqliteHelper sqliteHelper, ILogger logger)
        {
            _firearmsService = firearmsService;
            _sqliteHelper = sqliteHelper;
            _logger = logger;
            _printer = new FirearmsTablePrinter();
        }


        [Command("all")]
        public async Task<int> PrintFirearmsToConsole(CancellationToken cancellationToken = default)
         {
            AnsiConsole.Console.WriteAppInfo();
            await using SqliteConnection conn = await _sqliteHelper.GetDatabaseConnectionAsync(cancellationToken);
            Result<IEnumerable<Firearm>> firearms = await _firearmsService.GetFirearmsAsync(conn, cancellationToken);

            if (firearms.IsFailed)
            {
                _logger.Warning("Failed to retrieve firearms.");
                AnsiConsole.Console.WriteProblem("Failed to retrieve firearms.");
                return ReturnCodes.FAILURE;
            }

            if (!firearms.Value.Any())
            {
                AnsiConsole.Console.WriteWarning("No firearms found.");
                return ReturnCodes.SUCCESS;
            }

            _printer.SetFirearms(firearms.Value).Print(AnsiConsole.Console);
            AnsiConsole.Console.WriteSuccess("Firearms retrieved.");
            return ReturnCodes.SUCCESS;
        }
    }
}
