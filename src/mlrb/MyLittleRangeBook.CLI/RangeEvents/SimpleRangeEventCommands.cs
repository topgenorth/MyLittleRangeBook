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




        /// <summary>
        ///     Delete a range event from the database by ID.
        /// </summary>
        /// <param name="id">The ID of the range event to delete.</param>
        /// <param name="quiet">If set to true, then less verbose output.</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [Command("delete")]
        [UsedImplicitly]
        public async Task<int> DeleteRangeEvent(string id, bool quiet = false, CancellationToken ct = default)
        {
            // TODO [TO20260717] Need to delete the association with any firearms.
            if (!quiet)
            {
                CliDisplay.PrintCommandHeader($"Delete range event {id}");
            }

            int                  returnCode;
            DapperCommandContext context = await DapperCommandContext.NewAsync(SqliteHelper, ct).ConfigureAwait(false);
            try
            {
                // First, retrieve the event to ensure it exists
                Result<SimpleRangeEvent> getResult =
                    await _simpleRangeEventService.GetAsync(Guid.Parse(id), ct).ConfigureAwait(false);

                if (getResult.IsFailed)
                {
                    Logger.Warning("Could not find simple range event {id} for deletion.", id);
                    CliDisplay.PrintFailure("Could not find the requested range event.");
                    returnCode = FAILURE;
                }
                else
                {
                    // Delete the event
                    Result<bool> deleteResult = await _simpleRangeEventService.DeleteAsync(getResult.Value, ct)
                                                                              .ConfigureAwait(false);

                    if (deleteResult.IsSuccess)
                    {
                        CliDisplay.PrintSuccess($"Range event {id} deleted successfully.");
                        returnCode = SUCCESS;
                    }
                    else
                    {
                        Logger.Warning("Failed to delete simple range event {id}.", id);
                        CliDisplay.PrintFailure("Failed to delete the range event.");
                        returnCode = FAILURE;
                    }
                }
            }
            catch (Exception e)
            {
                returnCode = FAILURE;
                Logger.Error(e, e.Message);
                CliDisplay.PrintFailure("An error occurred while deleting the range event.");
            }

            System.Console.ReadKey();

            return returnCode;
        }
    }
}