using MyLittleRangeBook.Models;

namespace MyLittleRangeBook.EventSourcing
{
    /// <summary>
    ///     Abstract base class for event-sourced aggregates. Subclasses are responsible for
    ///     implementing the <see cref="Apply" /> method to mutate state in response to domain events.
    ///     The base class retains the uncommitted event list as well as the <see cref="Id" />,
    ///     <see cref="Version" />, <see cref="StreamType" /> and <see cref="DefaultStreamType" /> properties.
    /// </summary>
    public abstract class Aggregate: IIdentifiable
    {
        readonly List<IDomainEvent> _uncommitted = [];

        protected Aggregate() =>
            // ReSharper disable once VirtualMemberCallInConstructor
            StreamType = DefaultStreamType;

        /// <summary>
        ///     The default stream type for this aggregate. Subclasses must override this with a constant
        ///     string that uniquely identifies the type of stream the aggregate represents.
        /// </summary>
        public abstract string DefaultStreamType { get; }

        public MlrbId Id         { get; protected set; } = MlrbId.Empty;
        public int    Version    { get; protected set; } = -1;
        public string StreamType { get; protected set; }

        /// <summary>
        ///     Applies a domain event to mutate the aggregate state. Subclasses must implement this
        ///     to handle their specific event types.
        /// </summary>
        public abstract void Apply(IDomainEvent e);

        /// <summary>
        ///     Raises a new domain event: applies it to the state, appends it to the uncommitted list,
        ///     and increments the version.
        /// </summary>
        public void Raise(IDomainEvent e)
        {
            Apply(e);
            _uncommitted.Add(e);
            Version++;
        }

        /// <summary>
        ///     Returns and clears the list of uncommitted events.
        /// </summary>
        public IReadOnlyList<IDomainEvent> DequeueUncommittedEvents()
        {
            IDomainEvent[] events = _uncommitted.ToArray();
            _uncommitted.Clear();

            return events;
        }

        /// <summary>
        ///     Initializes the aggregate from an existing event stream (used during rehydration).
        /// </summary>
        protected void Hydrate(EventStreamRow streamRow)
        {
            Id         = streamRow.StreamId;
            StreamType = streamRow.StreamType;
            Version    = streamRow.Version;
        }

        /// <summary>
        ///     Replays a sequence of historical events against the aggregate via <see cref="Apply" />,
        ///     without enqueueing them as uncommitted. The aggregate's <see cref="Version" /> is expected
        ///     to have been set via <see cref="Hydrate" /> from the corresponding <see cref="EventStreamRow" />.
        /// </summary>
        protected internal void LoadFromHistory(IEnumerable<IDomainEvent> events)
        {
            foreach (IDomainEvent e in events)
            {
                Apply(e);
            }
        }
    }
}