using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using MyLittleRangeBook.Console;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;
using static MyLittleRangeBook.ReturnCodes;


namespace MyLittleRangeBook.RangeEvents
{
    [RegisterCommands("rangeevent")]
    public class SimpleRangeEventCommands : MlrbSqliteCommandBase
    {
        readonly ISimpleRangeEventListPrinter _printer;
        readonly ISimpleRangeEventService     _simpleRangeEventService;

        public SimpleRangeEventCommands(ILogger                      logger,
                                        ISqliteHelper                sqlitehelper,
                                        ICliDisplay                  cliDisplay,
                                        ISimpleRangeEventListPrinter printer,
                                        ISimpleRangeEventService     simpleRangeEventService) :
            base(logger, cliDisplay, sqlitehelper)
        {
            _printer                 = printer;
            _simpleRangeEventService = simpleRangeEventService;
        }

        [Command("export-to-csv")]
        [UsedImplicitly]
        public async Task<int> ExportToCsv(string?           file              = null, bool quiet = false,
                                           CancellationToken cancellationToken = default)
        {
            CliDisplay.PrintCommandHeader("Export range events to CSV.");
            DapperCommandContext context =
                await DapperCommandContext.NewAsync(SqliteHelper, cancellationToken).ConfigureAwait(false);


            int    returnCode;
            string csvFileName = file ?? Path.GetTempFileName();
            try
            {
                Result r = await _simpleRangeEventService.ExportToCsv(csvFileName, cancellationToken)
                                                         .ConfigureAwait(false);

                returnCode = r.IsSuccess ? SUCCESS : FAILURE;
            }
            catch (Exception ex)
            {
                CliDisplay.PrintFailure(ex.Message);
                returnCode = FAILURE;
            }

            if (returnCode == SUCCESS)
            {
                CliDisplay.PrintSuccess($"Range events exported to CSV successfully {csvFileName}.");
            }
            else
            {
                CliDisplay.PrintFailure("Failed to export range events to CSV.");
            }

            return returnCode;
        }





    }
}