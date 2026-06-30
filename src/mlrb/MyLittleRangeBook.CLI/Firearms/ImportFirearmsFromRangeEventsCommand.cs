using ConsoleAppFramework;
using FluentResults;
using JetBrains.Annotations;
using MyLittleRangeBook.EventSourcing;
using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.Persistence;

namespace MyLittleRangeBook
{
    public partial class UpdateFirearmsFromRangeEventsCommand
    {
        /// <summary>
        ///     This is a maintenance task. It will update the Firearms table based on what is in the SimpleRangeEvents table.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Command("import-from-range-events")]
        [UsedImplicitly]
        [Obsolete()]
        public async Task<int> ImportFirearmsFromRangeEvents(CancellationToken cancellationToken = default)
        {
            CliDisplay.PrintCommandHeader("Import new firearms from range events.");
            int returnCode;

            await using DapperCommandContext context = await DapperCommandContext
                                                            .NewAsync(SqliteHelper, cancellationToken, true)
                                                            .ConfigureAwait(false);
            try
            {
                IEnumerable<FirearmNameFromSimpleRangeEventRow> newFirearmNames =
                    await Commands.s_newFirearmNamesFromRangeEvents
                                  .QueryAsync<FirearmNameFromSimpleRangeEventRow>(context).ConfigureAwait(false);

                int count         = 0;
                int newNamesCount = 0;
                foreach (FirearmNameFromSimpleRangeEventRow f in newFirearmNames)
                {
                    newNamesCount++;

                    #region Step 1: Get the event stream
                    EventStreamRow? es = await _eventSourcingService.GetEventStream(context, f.FirearmId)
                                                                    .ConfigureAwait(false);

                    if (es is not null)
                    {
                        continue;
                    }

                    FirearmAggregate fa = FirearmAggregate.New(f.FirearmName, f.CreatedUtc);
                    fa.AddNote($"Imported from simple_range_event {f.SimpleRangeEventId}.",
                                     DateTimeOffset.UtcNow);

                    CliDisplay
                       .PrintWarning($"Will have to create the firearm '{f.FirearmName}' event stream from the range events");
                    #endregion

                    #region Step 2: Get round counts from simple range events, and update the aggregate.
                    List<(string FirearmId, string SimpleRangeEventId)> associations = [];
                    DapperCommandContext ctx = context with { Arguments = new { f.FirearmName } };
                    IEnumerable<RoundCountEventRow> shotsFired =
                        await Commands.s_rangeEventRoundCountsForFirearm.QueryAsync<RoundCountEventRow>(ctx)
                                      .ConfigureAwait(false);
                    foreach (RoundCountEventRow r in shotsFired)
                    {
                        fa.FirearmRoundCountChanged(r.RoundsFired, r.CreatedUtc);
                        associations.Add((fa.Id, r.SimpleRangeEventId));
                    }

                    Result upsertEventStreamResult = await FirearmAggregateRepository
                                                          .UpsertAsync(context, fa)
                                                          .ConfigureAwait(false);
                    if (upsertEventStreamResult.IsFailed)
                    {
                        Logger.Warning("Failed to create the firearm aggregate for '{FirearmName}' - skipped",
                                       f.FirearmName);
                        CliDisplay
                           .PrintWarning($"Failed to create the firearm aggregate for '{f.FirearmName}' - skipped.");
                        continue;
                    }

                    CliDisplay.PrintInfo($"Created the event stream for '{f.FirearmName}'.");
                    #endregion


                    #region Step 3: Upsert the firearm.
                    Firearm f2 = fa.ToFirearm();
                    Result<EntityId> firearmUpsertResult =
                        await FirearmsService.UpsertAsync(context, f2).ConfigureAwait(false);
                    if (firearmUpsertResult.IsFailed)
                    {
                        Logger.Warning("Failed to upsert firearm '{FirearmName}' - skipped.", f2.Name);
                        CliDisplay.PrintWarning($"Failed to upsert firearm '{f2.Name}' - skipped.");
                    }
                    else
                    {
                        count++;
                        CliDisplay.PrintInfo($"Save '{f2.Name}' in `firearms` table.");
                    }
                    #endregion


                    #region Step 4: Associate the range event to the firearm.
                    foreach (DapperCommandContext ctx2 in associations.Select(r => context with
                                                                               {
                                                                                   Arguments = new
                                                                                       {
                                                                                           r.FirearmId,
                                                                                           r.SimpleRangeEventId,
                                                                                       },
                                                                               }))
                    {
                        long result = await Commands.s_associateFirearmWithRangeEvent.ExecuteScalarAsync<long>(ctx2)
                                                    .ConfigureAwait(false);
                    }
                    #endregion
                }

                returnCode = ReturnCodes.SUCCESS;
                CliDisplay.PrintSuccess($"Imported {count}/{newNamesCount} new firearms.");
            }
            catch (Exception e)
            {
                returnCode = ReturnCodes.FAILURE;
                Logger.Error(e, "Unexpected exception trying to import firearms from range events.");
            }

            if (returnCode != ReturnCodes.SUCCESS)
            {
                await context.RollbackAsync().ConfigureAwait(false);
            }
            else
            {
                await context.CommitAsync().ConfigureAwait(false);
            }

            PressEnterToContinue();

            return returnCode;
        }
    }
}