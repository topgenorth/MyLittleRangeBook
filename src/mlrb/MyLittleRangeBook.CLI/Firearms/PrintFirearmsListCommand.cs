using System.ComponentModel.DataAnnotations.Schema;
using ConsoleAppFramework;
using Dapper;
using JetBrains.Annotations;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Console;
using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.Persistence.Sqlite;
using Spectre.Console.Rendering;

namespace MyLittleRangeBook
{
    /// <summary>
    ///     This will display a list o firearms and the number of rounds discharged to date.
    /// </summary>
    [RegisterCommands("firearms")]
    public class PrintFirearmsListCommand : MlrbFirearmsCommandBase
    {
        const string SELECT_FIREARM_ROUND_COUNT_SQL = """
                                                      SELECT firearm_name AS Name, round_count AS RoundCount FROM main.firearm_round_counts ORDER BY firearm_name;
                                                      """;

        internal class FirearmsTablePrinter : IConsolePrinter
        {
            IEnumerable<SimpleRoundCount> _firearms = [];

            public void Print(IAnsiConsole console) => console.Write(BuildRenderable());

            public IRenderable BuildRenderable()
            {
                Table table = new Table()
                             .Border(TableBorder.DoubleEdge)
                             .ShowRowSeparators()
                             .Expand()
                             .BorderColor(Color.White)
                             .AddColumn("Name",   col => col.Alignment(Justify.Left))
                             .AddColumn("Rounds", col => col.Alignment(Justify.Center).Width(6));

                foreach (SimpleRoundCount firearm in _firearms)
                {
                    table.AddRow(firearm.Name, firearm.RoundCount.ToString());
                }

                Panel p = new Panel(table).Expand().Border(BoxBorder.None);

                return p;
            }

            internal FirearmsTablePrinter SetFirearms(IEnumerable<SimpleRoundCount> firearms)
            {
                _firearms = firearms;

                return this;
            }
        }

        public record struct SimpleRoundCount( string Name, int RoundCount);

        readonly FirearmsTablePrinter _printer = new();
        readonly ISqliteHelper        _sqliteHelper;

        public PrintFirearmsListCommand(ILogger          logger,
                                        ICliDisplay      display,
                                        IFirearmsService firearmsService,
                                        ISqliteHelper    sqliteHelper) : base(logger, display, firearmsService) =>
            _sqliteHelper = sqliteHelper;

        /// <summary>
        ///     List all the active firearms.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Command("list")]
        [UsedImplicitly]
        public async Task<int> PrintFirearmsToConsole(CancellationToken cancellationToken = default)
        {
            CliDisplay.PrintCommandHeader("List firearms");

            try
            {
                await using SqliteConnection conn = await _sqliteHelper.GetDatabaseConnectionAsync(cancellationToken);

                IEnumerable<SimpleRoundCount> firearms =
                    await conn.QueryAsync<SimpleRoundCount>(SELECT_FIREARM_ROUND_COUNT_SQL, cancellationToken);

                _printer.SetFirearms(firearms).Print(AnsiConsole.Console);
                CliDisplay.PrintSuccess("Firearms listed.");

                return ReturnCodes.SUCCESS;
            }
            catch (Exception e)
            {
                CliDisplay.PrintFailure($"Failed to list firearms. {e.Message}");
                return ReturnCodes.FAILURE;
            }
        }
    }
}