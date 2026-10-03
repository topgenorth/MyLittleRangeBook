using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using DynamicData.Binding;
using DynamicData.Kernel;
using Fisher;
using FluentResults;
using MyLittleRangeBook.GUI.Messages;
using MyLittleRangeBook.RangeEvents;
using SharedControls.Controls;
using SharedControls.Helper;
using SharedControls.Services;

namespace MyLittleRangeBook.GUI.listSimpleRangeSession
{
    /// <summary>
    ///     ViewModel for managing the display and manipulation of SimpleRangeEvents.
    ///     Handles filtering, sorting, CRUD operations, and real-time updates of SimpleRangeEvents.
    ///     Implements reactive data binding using DynamicData for efficient UI updates.
    /// </summary>
    [UnconditionalSuppressMessage("Trimming", "IL2112",
                                  Justification =
                                      "We have all needed members added via DynamicallyAccessedMembers-Attribute")]
    [UnconditionalSuppressMessage("Trimming", "IL2026",
                                  Justification =
                                      "We have all needed members added via DynamicallyAccessedMembers-Attribute")]
    public partial class ListSimpleRangeSessionsViewModel : ViewModelBase,
                                                          IDialogParticipant,
                                                          IRecipient<UpdateDataMessage<SimpleRangeSession>>
    {
        readonly IDialogService                           _dialogService;
        readonly Func<IDialogParticipant, IDialogService> _dialogServiceFactory;
        readonly ILogger                                  _logger;
        readonly ISimpleRangeSessionService                 _rangeSessionService;
        readonly IDocumentSession                         _session;

        /// <summary>
        ///     Source cache for managing ManageSimpleRangeEventsVM instances with reactive updates.
        ///     Uses the SimpleRangeEvent ID as the key for efficient lookups and updates.
        /// </summary>
        readonly SourceCache<GUI.SimpleRangeSessionViewModel, Guid> _simpleRangeEventSourceCache = new(x => x.Id);

        /// <summary>
        ///     Read-only collection bound to the UI for displaying filtered and sorted SimpleRangeEvents.
        ///     Automatically updated through the reactive pipeline.
        /// </summary>
        readonly ReadOnlyObservableCollection<GUI.SimpleRangeSessionViewModel> _simpleRangeEvents;


        public ListSimpleRangeSessionsViewModel(ILogger logger,
                                              Func<IDialogParticipant,
                                                  IDialogService> dialogServiceFactory,
                                              ISimpleRangeSessionService rangeSessionService,
                                              IDocumentSession         session)
        {
            _dialogServiceFactory = dialogServiceFactory;
            _dialogService        = dialogServiceFactory(this);
            _logger               = logger;
            _rangeSessionService    = rangeSessionService;
            _session              = session;

            // Register for message notifications from other ViewModels
            WeakReferenceMessenger.Default.Register(this);

            // Get the current synchronization context for UI thread operations
            SynchronizationContext syncContext = SynchronizationContext.Current ??
                                                 throw new InvalidOperationException(
                                                  "No SynchronizationContext provided.");

            // Create reactive observable for text filtering with 300ms throttle to reduce frequent updates
            IObservable<Func<GUI.SimpleRangeSessionViewModel, bool>> filterByFirearmOrRangeObservable = this.ObserveValue(
                 nameof(FilterString),
                 () => FilterString)
               .Throttle(TimeSpan.FromMilliseconds(300))
               .DistinctUntilChanged()
               .Select(FilterByFirearmOrRangeObservable);

            // Create a reactive observable for sorting with a three-level sort priority
            IObservable<SortExpressionComparer<GUI.SimpleRangeSessionViewModel>> sortObservable = this
               .ObserveValue(nameof(SortExpression1), () => SortExpression1)
               .CombineLatest(
                              this.ObserveValue(nameof(SortExpression2), () => SortExpression2),
                              this.ObserveValue(nameof(SortExpression3), () => SortExpression3),
                              (s1,
                               s2,
                               s3) =>
                              {
                                  SortExpressionComparer<GUI.SimpleRangeSessionViewModel> comparer = s1.IsDescending
                                      ? SortExpressionComparer<GUI.SimpleRangeSessionViewModel>
                                         .Descending(s1.SortExpression)
                                      : SortExpressionComparer<GUI.SimpleRangeSessionViewModel>
                                         .Ascending(s1.SortExpression);

                                  comparer = s2.IsDescending
                                                 ? comparer.ThenByDescending(s2.SortExpression)
                                                 : comparer.ThenByAscending(s2.SortExpression);

                                  return s3.IsDescending
                                             ? comparer.ThenByDescending(s3.SortExpression)
                                             : comparer.ThenByAscending(s3.SortExpression);
                              }
                             )
               .Select(x => x);

            // Set up a reactive data pipeline: auto-refresh firearm name changes, apply filters and sorting
            _simpleRangeEventSourceCache.Connect()
                                        .AutoRefresh(
                                                     x => x.FirearmName,
                                                     propertyChangeThrottle: TimeSpan.FromMilliseconds(500))
                                        .Filter(filterByFirearmOrRangeObservable)
                                        .ObserveOn(syncContext)
                                        .SortAndBind(out _simpleRangeEvents, sortObservable)
                                        .Subscribe();

            // Load initial data from database
            _ = LoadSimpleRangeEvents();
        }

        /// <summary>
        ///     Public property exposing the filtered and sorted SimpleRangeEvents collection for UI binding.
        /// </summary>
        public ReadOnlyObservableCollection<GUI.SimpleRangeSessionViewModel> SimpleRangeEvents => _simpleRangeEvents;

        /// <summary>
        ///     Filter text for searching SimpleRangeEvents by firearm name or ange name.
        ///     Changes trigger reactive filtering with 300ms throttle.
        /// </summary>
        [ObservableProperty]
        public partial string? FilterString { get; set; }

        /// <summary>
        ///     Primary sort expression for SimpleRangeEvents (defaults to Event Date).
        ///     Changes trigger immediate re-sorting of the displayed items.
        /// </summary>
        [ObservableProperty]
        public partial SimpleRangeSessionsSortExpression SortExpression1 { get; set; } =
            SimpleRangeSessionsSortExpression.SortBySessionDateExpression;

        /// <summary>
        ///     Secondary sort expression for SimpleRangeEvents (defaults to Firearm Name).
        ///     Changes trigger immediate re-sorting of the displayed items.
        /// </summary>
        [ObservableProperty]
        public partial SimpleRangeSessionsSortExpression SortExpression2 { get; set; } =
            SimpleRangeSessionsSortExpression.SortByFirearmNameExpression;

        /// <summary>
        ///     Tertiary sort expression for SimpleRangeEvents (defaults to Range Name).
        ///     Changes trigger immediate re-sorting of the displayed items.
        /// </summary>
        [ObservableProperty]
        public partial SimpleRangeSessionsSortExpression SortExpression3 { get; set; } =
            SimpleRangeSessionsSortExpression.SortByRangeNameExpression;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ListSimpleRangeSessionsViewModel.DeleteSimpleRangeEventCommand), nameof(ListSimpleRangeSessionsViewModel.EditSimpleRangeEventCommand))]
        public partial GUI.SimpleRangeSessionViewModel? SelectedSimpleRangeEvent { get; set; }


        public void Receive(UpdateDataMessage<SimpleRangeSession> message)
        {
            SimpleRangeSession[] updatedEvents = message.ItemsAffected;
            switch (message.Action)
            {
                case UpdateAction.Added:
                case UpdateAction.Updated:
                    _simpleRangeEventSourceCache.AddOrUpdate(
                                                             updatedEvents
                                                                .Select(x => new GUI.SimpleRangeSessionViewModel(x)));

                    break;
                case UpdateAction.Removed:
                    _simpleRangeEventSourceCache.Remove(
                                                        updatedEvents.Select(x => new GUI.SimpleRangeSessionViewModel(x)));

                    break;
                case UpdateAction.Reset:
                    // [TO20260310] NOP.
                    break;
                case UpdateAction.Cancelled:
                    // [TO20261001] NOP
                    break;
                default:
                    throw new ArgumentException($"Unknown Update action: {message.Action}");
            }

            _ = RefreshSimpleRangeEventsList();
        }

        /// <summary>
        ///     Loads SimpleRangeEvents from the database and populates the source cache.
        ///     Creates ManageSimpleRangeEventsVM wrappers for each database item.
        /// </summary>
        async Task LoadSimpleRangeEvents(CancellationToken cancellationToken = default)
        {
            Result<IEnumerable<SimpleRangeSession>> r = await _rangeSessionService
                                                         .GetSimpleRangeEventsAsync(cancellationToken);

            if (r.IsSuccess)
            {
                IEnumerable<GUI.SimpleRangeSessionViewModel> items = r.Value
                                                                    .OrderByDescending(x => x.EventDate)
                                                                    .Select(x => new GUI.SimpleRangeSessionViewModel(x));
                _simpleRangeEventSourceCache.AddOrUpdate(items);
            }
            else
            {
                StringBuilder msg = new("There was a problem trying to get the range events.");
                r.Reasons.ForEach(x => msg.AppendLine(x.Message));
                _logger.Error(msg.ToString());
                _simpleRangeEventSourceCache.Clear();
            }
        }

        [RelayCommand]
        async Task AddNewSimpleRangeEventAsync() => await EditSimpleRangeEventAsync(new GUI.SimpleRangeSessionViewModel());

        bool CanEditOrDeleteSimpleRangeEvent(GUI.SimpleRangeSessionViewModel? simpleRangeEvent) =>
            simpleRangeEvent is not null;


        [RelayCommand(CanExecute = nameof(CanEditOrDeleteSimpleRangeEvent))]
        async Task DeleteSimpleRangeEventAsync(GUI.SimpleRangeSessionViewModel? simpleRangeEvent,
                                               CancellationToken              cancellationToken = default)
        {
            if (simpleRangeEvent is null)
            {
                return;
            }

            DialogResult result = await this.ShowOverlayDialogAsync<DialogResult>("Delete the Range Event",
                                      "Are you sure you want to delete this range event?",
                                      DialogCommands.YesNo);


            if (result == DialogResult.Yes)
            {
                // await using DapperCommandContext context =
                //     await DapperCommandContext.NewAsync(_sqliteHelper, cancellationToken, true);
                // SimpleRangeEvent sre       = simpleRangeEvent.ToSimpleRangeEvent();
                // var              firearmId = MlrbId.FromString(sre.FirearmName);

                // var r = await _simpleRangeEventDataProcessor
                //     .DeleteSimpleRangeEvent(context, sre)
                //     .ConfigureAwait(false);
                //
                //
                // if (r.IsSuccess)
                // {
                //     _simpleRangeEventSourceCache.Remove(simpleRangeEvent);
                //     await context.CommitAsync().ConfigureAwait(false);
                //     WeakReferenceMessenger.Default.Send(new UpdateDataMessage<SimpleRangeEvent>(
                //                                          UpdateAction.Removed, sre));
                //     _logger.Error("Range event deleted successfully.");
                //
                // }
                // else
                // {
                //     StringBuilder message = new("Could not delete the range event.");
                //     r.Reasons.ForEach(reason => message.AppendLine(reason.Message));
                //     _logger.Error(message.ToString());
                //
                //     await context.RollbackAsync().ConfigureAwait(false);
                // }
            }
        }

        [RelayCommand(CanExecute = nameof(CanEditOrDeleteSimpleRangeEvent))]
        async Task EditSimpleRangeEventAsync(GUI.SimpleRangeSessionViewModel? simpleRangeEvent)
        {
            if (simpleRangeEvent is null)
            {
                return;
            }

            EditSimpleRangeSessionViewModel vm = new(_dialogServiceFactory, _logger, _session, simpleRangeEvent,
                                                   _rangeSessionService);

            GUI.SimpleRangeSessionViewModel? result = await this.ShowOverlayDialogAsync<GUI.SimpleRangeSessionViewModel>(
                                                     simpleRangeEvent.IsNew
                                                         ? "Add Range Event"
                                                         : "Edit the Range Event",
                                                     vm);

            if (result != null)
            {
                // Update the item in the cache
                _simpleRangeEventSourceCache.AddOrUpdate(result);
            }
        }

        [RelayCommand]
        async Task RefreshSimpleRangeEventsList()
        {
            Guid previousSelectedId = SelectedSimpleRangeEvent.Id;
            _simpleRangeEventSourceCache.Clear();
            await LoadSimpleRangeEvents();

            Optional<GUI.SimpleRangeSessionViewModel> previousEvent = _simpleRangeEventSourceCache.Lookup(previousSelectedId);

            Dispatcher.UIThread.Post(() =>
                                         SelectedSimpleRangeEvent =
                                             previousEvent.HasValue ? previousEvent.Value : null);
        }

        /// <summary>
        ///     Creates a filter function for text-based filtering of SimpleRangeEvents.
        ///     Searches in both title and description fields using case-insensitive comparison.
        /// </summary>
        /// <param name="filterText">The text to search for (null/empty shows all items)</param>
        /// <returns>Filter function for use with DynamicData</returns>
        static Func<GUI.SimpleRangeSessionViewModel, bool> FilterByFirearmOrRangeObservable(string? filterText) =>
            item =>
            {
                // No filter text means this item should be visible
                if (string.IsNullOrWhiteSpace(filterText))
                {
                    return true;
                }

                // Search filter text in title and description (case-insensitive)
                string firearmName = item.FirearmName ?? string.Empty;
                string rangeName   = item.RangeName   ?? string.Empty;
                return firearmName.Contains(filterText, StringComparison.OrdinalIgnoreCase)
                       || rangeName.Contains(filterText, StringComparison.OrdinalIgnoreCase);
            };
    }
}