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
using MyLittleRangeBook.Database.Sqlite;
using MyLittleRangeBook.Gui.Database;
using MyLittleRangeBook.Gui.Messages;
using MyLittleRangeBook.Gui.Models;
using SharedControls.Controls;
using SharedControls.Helper;
using SharedControls.Services;

namespace MyLittleRangeBook.Gui.ViewModels
{
    [UnconditionalSuppressMessage("Trimming", "IL2112",
        Justification = "We have all needed members added via DynamicallyAccessedMembers-Attribute")]
    [UnconditionalSuppressMessage("Trimming", "IL2026",
        Justification = "We have all needed members added via DynamicallyAccessedMembers-Attribute")]
    public partial class ManageFirearmsViewModel : ViewModelBase, IDialogParticipant,
        IRecipient<UpdateDataMessage<Firearm>>
    {
        readonly ISqliteHelper _sqliteHelper;

        readonly SourceCache<FirearmViewModel, long> _firearmViewModelCache = new(x => x.Id ?? -1);

        readonly ReadOnlyObservableCollection<FirearmViewModel> _firearmViewModels;

        public ManageFirearmsViewModel(ISqliteHelper sqliteHelper)
        {
            _sqliteHelper = sqliteHelper;
            WeakReferenceMessenger.Default.Register(this);

            // Get the current synchronization context for UI thread operations
            var syncContext = SynchronizationContext.Current ??
                              throw new InvalidOperationException("No SynchronizationContext provided.");

            // Create reactive observable for text filtering with 300ms throttle to reduce frequent updates
            var filterByName = this.ObserveValue(nameof(FilterString),
                    () => FilterString)
                .Throttle(TimeSpan.FromMilliseconds(300))
                .DistinctUntilChanged()
                .Select(FilterByNameObservable);


            // Set up a reactive data pipeline: auto-refresh firearm name changes, apply filters and sorting
            _firearmViewModelCache.Connect()
                .AutoRefresh(
                    x => x.Name,
                    propertyChangeThrottle: TimeSpan.FromMilliseconds(500))
                .Filter(filterByName)
                .ObserveOn(syncContext)
                .SortBy(x => x.Name, resetThreshold: 500)
                .Bind(out _firearmViewModels)
                .Subscribe();

            _ = LoadDataAsync();
        }

        public ReadOnlyObservableCollection<FirearmViewModel> FirearmViewModels => _firearmViewModels;

        /// <summary>
        ///     The string used to filter the list of firearms.
        /// </summary>
        [ObservableProperty]
        public partial string? FilterString { get; set; }

        [ObservableProperty]
        public partial FirearmsSortExpression SortExpression1 { get; set; } =
            FirearmsSortExpression.SortByName;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(DeleteFirearmCommand), nameof(EditFirearmCommand))]
        public partial FirearmViewModel? SelectedFirearm { get; set; }

        public void Receive(UpdateDataMessage<Firearm> message)
        {
            var updateEvents = message.ItemsAffected;
            switch (message.Action)
            {
                case UpdateAction.Added:
                case UpdateAction.Updated:
                    _firearmViewModelCache.AddOrUpdate(
                        updateEvents.Select(x => new FirearmViewModel(x)));

                    break;
                case UpdateAction.Removed:
                    _firearmViewModelCache.Remove(
                        updateEvents.Select(x => new FirearmViewModel(x)));

                    break;
                case UpdateAction.Reset:
                    // [TO20260319] NOP.
                    break;
                default:
                    throw new ArgumentException($"Unknown Update action: {message.Action}");
            }

            _ = RefreshAsync();
        }

        [RelayCommand]
        async Task LoadDataAsync()
        {
            await using var connection = await _sqliteHelper.OpenSqliteConnectionToFileAsync();
            var firearms = await DatabaseHelper.GetFirearmsAsync(connection);
            _firearmViewModelCache.AddOrUpdate(firearms.Select(x => new FirearmViewModel(x)));
        }

        [RelayCommand]
        async Task AddNewFirearmAsync()
        {
            var firearm = new Firearm();

            await EditFirearmAsync(new FirearmViewModel(firearm));
        }

        bool CanEditOrDeleteFirearm(FirearmViewModel? firearm)
        {
            return firearm != null;
        }

        [RelayCommand(CanExecute = nameof(CanEditOrDeleteFirearm))]
        async Task DeleteFirearmAsync(FirearmViewModel? firearm)
        {
            if (firearm is null)
            {
                return;
            }

            var result = await this.ShowOverlayDialogAsync<DialogResult>("Delete the Firearm",
                "Are you sure you want to delete this Firearm?", DialogCommands.YesNoCancel);

            await using var connection = await _sqliteHelper.OpenSqliteConnectionToFileAsync();
            if (result == DialogResult.Yes && await firearm.ToFirearm().DeleteAsync(connection))
            {
                _firearmViewModelCache.Remove(firearm);
            }
        }

        [RelayCommand(CanExecute = nameof(CanEditOrDeleteFirearm))]
        async Task EditFirearmAsync(FirearmViewModel? firearm)
        {
            if (firearm is null)
            {
                return;
            }

            var vm = new EditFirearmViewModel(firearm.CloneFirearmViewModel());
            var result = await this.ShowDialogWindow<FirearmViewModel>(
                "Edit firearm", firearm);

            if (result is not null)
            {
                _firearmViewModelCache.AddOrUpdate(result);
            }
        }

        [RelayCommand]
        async Task RefreshAsync()
        {
            var prevId = SelectedFirearm?.Id ?? -1;
            _firearmViewModelCache.Clear();
            await LoadDataAsync();
            var prevFirearm = _firearmViewModelCache.Lookup(prevId);

            Dispatcher.UIThread.Post(() =>
                SelectedFirearm = prevFirearm.HasValue ? prevFirearm.Value : null);
        }

        /// <summary>
        ///     Creates a filter function for text-based filtering of SimpleRangeEvents.
        ///     Searches in both title and description fields using case-insensitive comparison.
        /// </summary>
        /// <param name="filterText">The text to search for (null/empty shows all items)</param>
        /// <returns>Filter function for use with DynamicData</returns>
        static Func<FirearmViewModel, bool> FilterByNameObservable(string? filterText)
        {
            return item =>
            {
                // No filter text means this item should be visible
                if (string.IsNullOrWhiteSpace(filterText))
                {
                    return true;
                }

                // Search filter text in title and description (case-insensitive)
                return item.Name?.Contains(filterText, StringComparison.OrdinalIgnoreCase) ?? false;
            };
        }
    }
}
