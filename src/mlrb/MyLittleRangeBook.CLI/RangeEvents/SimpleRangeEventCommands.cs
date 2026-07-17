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
        public async Task<int> ExportToCsv(string? file = null, bool quiet = false, CancellationToken cancellationToken = default)
        {
            CliDisplay.PrintCommandHeader("Export range events to CSV.");
            DapperCommandContext context = await DapperCommandContext.NewAsync(SqliteHelper, cancellationToken).ConfigureAwait(false);


            int    returnCode;
            string csvFileName = file ?? Path.GetTempFileName();
            try
            {
                Result r = await _simpleRangeEventService.ExportToCsv(context,csvFileName).ConfigureAwait(false);

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
        ///     Display a single range event.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="quiet">If set to true, then less verbose, single line output.</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [Command("show")]
        [UsedImplicitly]
        public async Task<int> DisplayOneRangeEvent(string id, bool quiet = false, CancellationToken ct = default)
        {
            if (quiet)
            {
                CliDisplay.PrintCommandHeader();
            }
            else
            {
                CliDisplay.PrintCommandHeader($"Show range event {id}");
            }

            int                  returnCode;
            DapperCommandContext context = await DapperCommandContext.NewAsync(SqliteHelper, ct).ConfigureAwait(false);

            try
            {
                Result<SimpleRangeEvent> result =
                    await _simpleRangeEventService.GetAsync(context, id).ConfigureAwait(false);

                if (result.IsFailed)
                {
                    Logger.Warning("Could not find simple range event {id} for display.", id);
                    CliDisplay.PrintFailure("Could not find the request range event.");
                    returnCode = FAILURE;
                }
                else
                {
                    SimpleRangeEventPrinter2 p = new();
                    p.Print(CliDisplay.Console, result.Value!, quiet);
                    CliDisplay.PrintSuccess("Range event displayed successfully.");
                    returnCode = SUCCESS;
                }
            }
            catch (Exception e)
            {
                returnCode = FAILURE;
                Logger.Error(e, e.Message);
                CliDisplay.PrintFailure("An error occurred while displaying the range event.");
            }

            System.Console.ReadKey();

            return returnCode;
        }

        /// <summary>
        ///     List all the range events in the database.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Command("list")]
        [UsedImplicitly]
        public async Task<int> ListRangeEvents(CancellationToken cancellationToken)
        {
            CliDisplay.PrintCommandHeader("List range events.");
            DapperCommandContext context =
                await DapperCommandContext.NewAsync(SqliteHelper, cancellationToken).ConfigureAwait(false);
            Result<IEnumerable<SimpleRangeEvent>> rangeEvents =
                await _simpleRangeEventService.GetSimpleRangeEventsAsync(context)
                                              .ConfigureAwait(false);
            if (rangeEvents.IsFailed)
            {
                CliDisplay.PrintFailure("Could not retrieve the list.");
                Logger.Warning("Failed to retrieve list from database.");

                return FAILURE;
            }

            await _printer.Start().ConfigureAwait(false);
            foreach (SimpleRangeEvent sre in rangeEvents.Value)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Logger.Warning("Operation cancelled by user.");
                    CliDisplay.PrintFailure("Operation cancelled.");
                    await _printer.Finish().ConfigureAwait(false);

                    return COMMAND_CANCELLED;
                }

                await _printer.AddRow(sre).ConfigureAwait(false);
            }

            await _printer.Finish().ConfigureAwait(false);

            System.Console.ReadKey();

            return SUCCESS;
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
                    await _simpleRangeEventService.GetAsync(context, id).ConfigureAwait(false);

                if (getResult.IsFailed)
                {
                    Logger.Warning("Could not find simple range event {id} for deletion.", id);
                    CliDisplay.PrintFailure("Could not find the requested range event.");
                    returnCode = FAILURE;
                }
                else
                {
                    // Delete the event
                    Result<bool> deleteResult = await _simpleRangeEventService.DeleteAsync(context, getResult.Value)
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