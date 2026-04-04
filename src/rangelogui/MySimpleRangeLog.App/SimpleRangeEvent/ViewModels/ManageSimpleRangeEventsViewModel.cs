using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using DynamicData.Binding;
using MyLittleRangeBook.Database.Sqlite;
using MyLittleRangeBook.Gui.Database;
using MyLittleRangeBook.Gui.Messages;
using MyLittleRangeBook.Gui.Models;
using SharedControls.Controls;
using SharedControls.Helper;
using SharedControls.Services;

namespace MyLittleRangeBook.Gui.ViewModels
{
    /// <summary>
    ///     ViewModel for managing the display and manipulation of SimpleRangeEvents.
    ///     Handles filtering, sorting, CRUD operations, and real-time updates of SimpleRangeEvents.
    ///     Implements reactive data binding using DynamicData for efficient UI updates.
    /// </summary>
    [UnconditionalSuppressMessage("Trimming", "IL2112",
        Justification = "We have all needed members added via DynamicallyAccessedMembers-Attribute")]
    [UnconditionalSuppressMessage("Trimming", "IL2026",
        Justification = "We have all needed members added via DynamicallyAccessedMembers-Attribute")]
    public partial class ManageSimpleRangeEventsViewModel : ViewModelBase, IDialogParticipant,
        IRecipient<UpdateDataMessage<SimpleRangeEvent>>
    {
        /// <summary>
        ///     Read-only collection bound to the UI for displaying filtered and sorted SimpleRangeEvents.
        ///     Automatically updated through the reactive pipeline.
        /// </summary>
        readonly ReadOnlyObservableCollection<SimpleRangeEventViewModel> _simpleRangeEvents;

        /// <summary>
        ///     Source cache for managing ManageSimpleRangeEventsVM instances with reactive updates.
        ///     Uses the SimpleRangeEvent ID as the key for efficient lookups and updates.
        /// </summary>
        readonly SourceCache<SimpleRangeEventViewModel, long> _simpleRangeEventSourceCache = new(x => x.Id ?? -1);

        readonly ISqliteHelper _sqliteHelper;

        public ManageSimpleRangeEventsViewModel(ISqliteHelper sqliteHelper)
        {
            _sqliteHelper = sqliteHelper;
            // Register for message notifications from other ViewModels
            WeakReferenceMessenger.Default.Register(this);

            // Get the current synchronization context for UI thread operations
            var syncContext = SynchronizationContext.Current ??
                              throw new InvalidOperationException("No SynchronizationContext provided.");

            // Create reactive observable for text filtering with 300ms throttle to reduce frequent updates
            var filterByFirearmOrRangeObservable = this.ObserveValue(nameof(FilterString),
                    () => FilterString)
                .Throttle(TimeSpan.FromMilliseconds(300))
                .DistinctUntilChanged()
                .Select(FilterByFirearmOrRangeObservable);

            // Create a reactive observable for sorting with a three-level sort priority
            var sortObservable = this.ObserveValue(nameof(SortExpression1), () => SortExpression1)
                .CombineLatest(
                    this.ObserveValue(nameof(SortExpression2), () => SortExpression2),
                    this.ObserveValue(nameof(SortExpression3), () => SortExpression3),
                    (s1,
                        s2,
                        s3) => SortExpressionComparer<SimpleRangeEventViewModel>
                        .Ascending(s1.SortExpression)
                        .ThenByAscending(s2.SortExpression)
                        .ThenByAscending(s3.SortExpression)
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
            _ = LoadDataAsync();
        }

        /// <summary>
        ///     Public property exposing the filtered and sorted SimpleRangeEvents collection for UI binding.
        /// </summary>
        public ReadOnlyObservableCollection<SimpleRangeEventViewModel> SimpleRangeEvents => _simpleRangeEvents;

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
        public partial SimpleRangeEventsSortExpression SortExpression1 { get; set; } =
            SimpleRangeEventsSortExpression.SortByEventDateExpression;

        /// <summary>
        ///     Secondary sort expression for SimpleRangeEvents (defaults to Firearm Name).
        ///     Changes trigger immediate re-sorting of the displayed items.
        /// </summary>
        [ObservableProperty]
        public partial SimpleRangeEventsSortExpression SortExpression2 { get; set; } =
            SimpleRangeEventsSortExpression.SortByFirearmNameExpression;

        /// <summary>
        ///     Tertiary sort expression for SimpleRangeEvents (defaults to Range Name).
        ///     Changes trigger immediate re-sorting of the displayed items.
        /// </summary>
        [ObservableProperty]
        public partial SimpleRangeEventsSortExpression SortExpression3 { get; set; } =
            SimpleRangeEventsSortExpression.SortByRangeNameExpression;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteSimpleRangeEventCommand), nameof(EditSimpleRangeEventCommand))]
        public partial SimpleRangeEventViewModel? SelectedSimpleRangeEvent { get; set; }


        public void Receive(UpdateDataMessage<SimpleRangeEvent> message)
        {
            var updatedEvents = message.ItemsAffected;
            switch (message.Action)
            {
                case UpdateAction.Added:
                case UpdateAction.Updated:
                    _simpleRangeEventSourceCache.AddOrUpdate(
                        updatedEvents.Select(x => new SimpleRangeEventViewModel(x)));

                    break;
                case UpdateAction.Removed:
                    _simpleRangeEventSourceCache.Remove(
                        updatedEvents.Select(x => new SimpleRangeEventViewModel(x)));

                    break;
                case UpdateAction.Reset:
                    // [TO20260310] NOP.
                    break;
                default:
                    throw new ArgumentException($"Unknown Update action: {message.Action}");
            }

            _ = RefreshAsync();
        }

        /// <summary>
        ///     Loads SimpleRangeEvents from the database and populates the source cache.
        ///     Creates ManageSimpleRangeEventsVM wrappers for each database item.
        /// </summary>
        async Task LoadDataAsync()
        {
            await using var connection = await _sqliteHelper.OpenSqliteConnectionToFileAsync();

            var simpleRangeEvents = await DatabaseHelper.GetSimpleRangeEventsAsync(connection);

            // Convert database items to ViewModels and add to cache
            _simpleRangeEventSourceCache.AddOrUpdate(simpleRangeEvents.Select(x => new SimpleRangeEventViewModel(x)));
        }

        [RelayCommand]
        async Task AddNewSimpleRangeEventAsync()
        {
            var rangeEvent = new SimpleRangeEvent
            {
                Created = DateTimeOffset.UtcNow, Modified = DateTimeOffset.UtcNow, EventDate = DateTime.UtcNow
            };

            await EditSimpleRangeEventAsync(new SimpleRangeEventViewModel(rangeEvent));
        }

        bool CanEditOrDeleteSimpleRangeEvent(SimpleRangeEventViewModel? simpleRangeEvent)
        {
            return simpleRangeEvent is not null;
        }


        [RelayCommand(CanExecute = nameof(CanEditOrDeleteSimpleRangeEvent))]
        async Task DeleteSimpleRangeEventAsync(SimpleRangeEventViewModel? simpleRangeEvent)
        {
            if (simpleRangeEvent is null)
            {
                return;
            }

            var result = await this.ShowOverlayDialogAsync<DialogResult>("Delete the Range Event",
                "Are you sure you want to delete this range event?", DialogCommands.YesNoCancel);

            await using var connection = await _sqliteHelper.OpenSqliteConnectionToFileAsync();
            if (result == DialogResult.Yes && await simpleRangeEvent.ToSimpleRangeEvent().DeleteAsync(connection))
            {
                _simpleRangeEventSourceCache.Remove(simpleRangeEvent);
            }
        }

        [RelayCommand(CanExecute = nameof(CanEditOrDeleteSimpleRangeEvent))]
        async Task EditSimpleRangeEventAsync(SimpleRangeEventViewModel? simpleRangeEvent)
        {
            if (simpleRangeEvent is null)
            {
                return;
            }

            var vm = new EditSimpleRangeEventViewModel(simpleRangeEvent.CloneSimpleRangeEventViewModel());
            var result = await this.ShowOverlayDialogAsync<SimpleRangeEventViewModel>(
                "Edit the Range Event",
                vm);

            if (result != null)
            {
                // Update the item in the cache
                _simpleRangeEventSourceCache.AddOrUpdate(result);
            }
        }

        [RelayCommand]
        async Task RefreshAsync()
        {
            var previousSelectedId = SelectedSimpleRangeEvent?.Id ?? -1;
            _simpleRangeEventSourceCache.Clear();
            await LoadDataAsync();

            var previousEvent = _simpleRangeEventSourceCache.Lookup(previousSelectedId);

            Dispatcher.UIThread.Post(() =>
                SelectedSimpleRangeEvent = previousEvent.HasValue ? previousEvent.Value : null);
        }

        /// <summary>
        ///     Creates a filter function for text-based filtering of SimpleRangeEvents.
        ///     Searches in both title and description fields using case-insensitive comparison.
        /// </summary>
        /// <param name="filterText">The text to search for (null/empty shows all items)</param>
        /// <returns>Filter function for use with DynamicData</returns>
        static Func<SimpleRangeEventViewModel, bool> FilterByFirearmOrRangeObservable(string? filterText)
        {
            return item =>
            {
                // No filter text means this item should be visible
                if (string.IsNullOrWhiteSpace(filterText))
                {
                    return true;
                }

                // Search filter text in title and description (case-insensitive)
                return (item.FirearmName?.Contains(filterText, StringComparison.OrdinalIgnoreCase) ?? false)
                       || (item.RangeName?.Contains(filterText, StringComparison.OrdinalIgnoreCase) ?? false);
            };
        }
    }
}
