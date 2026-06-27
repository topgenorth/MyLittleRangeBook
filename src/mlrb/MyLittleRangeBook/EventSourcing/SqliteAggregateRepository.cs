using MyLittleRangeBook.Models;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;

namespace MyLittleRangeBook.EventSourcing
{
    /// <summary>
    ///     Generic SQLite-backed repository for any <see cref="Aggregate" /> subclass. It persists
    ///     uncommitted events to the <c>events</c> table and upserts the corresponding row in the
    ///     <c>event_streams</c> table.
    /// </summary>
    /// <typeparam name="TAggregate">The concrete aggregate type.</typeparam>
    public abstract class SqliteAggregateRepository<TAggregate> : ISqliteAggregateRepository<TAggregate>
        where TAggregate : Aggregate
    {
        readonly           Func<EventStreamRow, TAggregate> _createFromStream;
        readonly           IEventSerializer                 _eventSerializer;
        readonly           IEventSourcingService            _eventSourcingService;
        readonly           string                           _streamType;
        protected readonly ISqliteHelper                    SqliteHelper;

        /// <param name="sqliteHelper">SQLite connection factory.</param>
        /// <param name="eventSerializer">Serializer used to (de)serialize <see cref="IDomainEvent" /> instances.</param>
        /// <param name="streamType">The stream type identifier used in the <c>event_streams</c>/<c>events</c> tables.</param>
        /// <param name="createFromStream">Factory invoked when rehydrating an aggregate from an existing event stream.</param>
        /// <param name="eventSourcingService"></param>
        protected SqliteAggregateRepository(ISqliteHelper                    sqliteHelper,
                                            IEventSerializer                 eventSerializer,
                                            string                           streamType,
                                            Func<EventStreamRow, TAggregate> createFromStream,
                                            IEventSourcingService            eventSourcingService)
        {
            SqliteHelper          = sqliteHelper;
            _eventSerializer      = eventSerializer;
            _streamType           = streamType;
            _createFromStream     = createFromStream;
            _eventSourcingService = eventSourcingService;
        }

        /// <summary>
        ///     Retrieves an aggregate by its unique identifier from the event sourcing store. Does not load any prior events.
        /// </summary>
        /// <param name="context">The database command context, including connection, transaction (if any), and cancellation token.</param>
        /// <param name="id">The unique identifier of the aggregate to retrieve.</param>
        /// <returns>A result containing the aggregate if found, or <c>null</c> if no events exist for the specified identifier.</returns>
        public virtual async Task<Result<TAggregate?>> GetAsync(DapperCommandContext context, MlrbId id)
        {
            try
            {
                EventStreamRow? stream = await _eventSourcingService.GetEventStream(context, id).ConfigureAwait(false);
                if (stream is null)
                {
                    // [TO20260530] We couldn't find the stream; this is okay because it means this is a new thing.
                    Success reason = new Success("No event stream found").Enrich(id);
                    return new Result<TAggregate?>()
                          .WithValue(null)
                          .WithReason(reason);
                }

                TAggregate aggregate = _createFromStream(stream.Value);

                IEnumerable<IDomainEvent> domainEvents =
                    await _eventSourcingService.GetDomainEvents(context, id).ConfigureAwait(false);
                if (!domainEvents.Any())
                {
                    // [TO20260530] No events; this is okay because it means this is a new thing.
                    Success reason = new Success("No events found").Enrich(id);
                    return new Result<TAggregate?>()
                          .WithValue(null)
                          .WithReason(reason);
                }

                aggregate.LoadFromHistory(domainEvents);

                return Result.Ok<TAggregate?>(aggregate);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                return e.FailWithException();
            }
        }

        public async Task<Result<IEnumerable<IDomainEvent>>> GetDomainEvents(
            DapperCommandContext context, MlrbId streamId)
        {
            try
            {
                IEnumerable<IDomainEvent> events =
                    await _eventSourcingService.GetDomainEvents(context, streamId).ConfigureAwait(false);
                return Result.Ok(events);
            }
            catch (Exception e)
            {
                Error err = e.ToError().Enrich(streamId);
                return Result.Fail(err);
            }
        }


        /// <summary>
        ///     Upserts the specified aggregate into the database using the provided Dapper command context.
        ///     If the aggregate does not exist, it will be inserted; if it exists, it will be updated.
        /// </summary>
        /// <param name="context">The Dapper command context containing the database connection and transaction information.</param>
        /// <param name="aggregate">The aggregate entity to upsert.</param>
        /// <param name="metadataJson">Metadata for the event stream, in JSON format.</param>
        /// <returns>A result indicating the success or failure of the operation.</returns>
        public async Task<Result> UpsertAsync(DapperCommandContext context,
                                              TAggregate           aggregate,
                                              string?              metadataJson = null)
        {
            IReadOnlyList<IDomainEvent> pendingEvents = aggregate.DequeueUncommittedEvents();
            if (pendingEvents.Count == 0)
            {
                return Result.Ok();
            }

            MlrbId        streamId = aggregate.Id;
            List<IReason> reasons  = [];
            try
            {
                int? currentVersion = await GetStreamVersion(context, streamId).ConfigureAwait(false);
                int  nextVersion;

                #region figure out what the next version number of the stream should be, and do a crude concurrency check.
                if (currentVersion is null)
                {
                    nextVersion = 0;
                    reasons.Add(new Success("CurrentVersion is 0."));
                }
                else
                {
                    int expectedVersion = aggregate.Version - pendingEvents.Count;
                    if (currentVersion.Value != expectedVersion)
                    {
                        reasons.Add(new
                                        Success($"Concurrency conflict detected for stream {streamId}. Expected version {expectedVersion}, but actual version is {currentVersion}."));
                        return new Result().WithReasons(reasons);
                    }

                    nextVersion = currentVersion.Value + 1;
                    reasons.Add(new
                                    Success($"CurrentVersion is {currentVersion} and expectedVersion is {expectedVersion}"));
                }
                #endregion

                foreach (IDomainEvent evt in pendingEvents)
                {
                    await _eventSourcingService.InsertDomainEvent(context,
                                                                  streamId,
                                                                  _streamType,
                                                                  evt, nextVersion)
                                               .ConfigureAwait(false);
                    reasons.Add(new Success($"Inserted event {evt.GetType().Name} with version {nextVersion}"));
                    nextVersion++;
                }

                await _eventSourcingService.UpsertEventStream(context,
                                                              aggregate,
                                                              _streamType,
                                                              nextVersion - 1,
                                                              metadataJson
                                                             )
                                           .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Error err = e.ToError("Failed to upsert aggregate").Enrich(aggregate.Id);
                return Result.Fail(err);
            }

            return Result.Ok().WithReasons(reasons);
        }


        async Task<int?> GetStreamVersion(DapperCommandContext context,
                                          string               streamId)
        {
            DapperCommandContext ctx            = context with { Arguments = new { StreamId = streamId } };
            DapperCommand        versionCmd     = new("SELECT version from event_streams WHERE id=@StreamId;");
            int?                 currentVersion = await versionCmd.ExecuteScalarAsync<int?>(ctx).ConfigureAwait(false);

            return currentVersion;
        }
    }
}