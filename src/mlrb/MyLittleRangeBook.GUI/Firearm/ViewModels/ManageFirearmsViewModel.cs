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
using DynamicData.Kernel;
using FluentResults;
using Microsoft.Data.Sqlite;
using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.GUI.Messages;
using MyLittleRangeBook.GUI.Services;
using MyLittleRangeBook.Persistence;
using MyLittleRangeBook.Persistence.Sqlite;
using MyLittleRangeBook.RangeEvents;
using SharedControls.Controls;
using SharedControls.Helper;
using SharedControls.Services;

namespace MyLittleRangeBook.GUI.ViewModels
{
    [UnconditionalSuppressMessage("Trimming", "IL2112",
                                  Justification =
                                      "We have all needed members added via DynamicallyAccessedMembers-Attribute")]
    [UnconditionalSuppressMessage("Trimming", "IL2026",
                                  Justification =
                                      "We have all needed members added via DynamicallyAccessedMembers-Attribute")]
    public partial class ManageFirearmsViewModel : ViewModelBase, IDialogParticipant,
                                                   IRecipient<UpdateDataMessage<Firearm>>,
                                                   IRecipient<UpdateDataMessage<SimpleRangeEvent>>
    {
        readonly IDialogService                           _dialogService;
        readonly Func<IDialogParticipant, IDialogService> _dialogServiceFactory;
        readonly IFirearmsService                         _firearmsDbService;
        readonly SourceCache<FirearmViewModel, long>      _firearmViewModelCache = new(x => x.Id ?? -1);

        readonly ReadOnlyObservableCollection<FirearmViewModel> _firearmViewModels;
        readonly ILogger                                        _logger;
        readonly ISimpleRangeEventRepository                    _rangeEventRepo;
        readonly ISqliteHelper                                  _sqliteHelper;

        public ManageFirearmsViewModel(IFirearmsService                         firearmsDbService,
                                       ISimpleRangeEventRepository              rangeEventRepo,
                                       Func<IDialogParticipant, IDialogService> dialogServiceFactory,
                                       ISqliteHelper                            sqliteHelper,
                                       ILogger                                  logger)
        {
            _sqliteHelper         = sqliteHelper;
            _firearmsDbService    = firearmsDbService;
            _rangeEventRepo       = rangeEventRepo;
            _dialogServiceFactory = dialogServiceFactory;
            _dialogService        = dialogServiceFactory(this);
            _logger               = logger;

            // Register for message notifications from other ViewModels
            WeakReferenceMessenger.Default.Register<UpdateDataMessage<Firearm>>(this);
            WeakReferenceMessenger.Default.Register<UpdateDataMessage<SimpleRangeEvent>>(this);

            // Get the current synchronization context for UI thread operations
            SynchronizationContext syncContext = SynchronizationContext.Current ??
                                                 throw new InvalidOperationException(
                                                  "No SynchronizationContext provided.");

            // Create reactive observable for text filtering with 300ms throttle to reduce frequent updates
            IObservable<Func<FirearmViewModel, bool>> filterByName = this.ObserveValue(nameof(FilterString),
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
            Firearm[] updateEvents = message.ItemsAffected;
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

        public void Receive(UpdateDataMessage<SimpleRangeEvent> message) => _ = RefreshAsync();

        [RelayCommand]
        async Task LoadDataAsync(CancellationToken cancellationToken = default)
        {
            await using var ctx = await DapperCommandContext.NewAsync(_sqliteHelper, cancellationToken).ConfigureAwait(false);
            try
            {

                Result<IEnumerable<Firearm>> result = await _firearmsDbService.GetFirearmsAsync(ctx);

                if (result.IsSuccess)
                {
                    _firearmViewModelCache.AddOrUpdate(result.Value.Select(x => new FirearmViewModel(x)));
                }
                else
                {
                    StringBuilder msg = new("There was a problem trying to get the range events.");
                    result.Reasons.ForEach(x => msg.AppendLine(x.Message));
                    _logger.Error(msg.ToString());
                    _firearmViewModelCache.Clear();
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to load firearms.");
            }
        }

        [RelayCommand]
        async Task AddNewFirearmAsync()
        {
            Firearm firearm = new();

            await EditFirearmAsync(new FirearmViewModel(firearm));
        }

        [RelayCommand(CanExecute = nameof(CanEditOrDeleteFirearm))]
        async Task AddRangeEventForFirearmAsync(FirearmViewModel? firearm)
        {
            if (firearm is null)
            {
                return;
            }

            SimpleRangeEvent rangeEvent = new()
                                          {
                                              FirearmName = firearm.Name ?? string.Empty,
                                              Created     = DateTimeOffset.UtcNow,
                                              Modified    = DateTimeOffset.UtcNow,
                                              EventDate   = DateTime.UtcNow,
                                          };

            EditSimpleRangeEventViewModel vm = new(new SimpleRangeEventViewModel(rangeEvent), _logger,
                                                   _dialogServiceFactory, _rangeEventRepo, _sqliteHelper);

            await this.ShowOverlayDialogAsync<SimpleRangeEventViewModel>(
                                                                         "Add Range Event",
                                                                         vm);
        }

        bool CanEditOrDeleteFirearm(FirearmViewModel? firearm) => firearm != null;

        [RelayCommand(CanExecute = nameof(CanEditOrDeleteFirearm))]
        async Task DeleteFirearmAsync(FirearmViewModel? firearm, CancellationToken cancellationToken = default)
        {
            if (firearm is null)
            {
                return;
            }

            DialogResult result = await this.ShowOverlayDialogAsync<DialogResult>("Delete the Firearm",
                                      "Are you sure you want to delete this Firearm?", DialogCommands.YesNoCancel);

            if (result == DialogResult.Yes)
            {
                try
                {
                    await using SqliteConnection connection =
                        await _sqliteHelper.GetDatabaseConnectionAsync(cancellationToken);
                    DapperCommandContext ctx = new(connection, null, cancellationToken);
                    Result<bool>         r   = await _firearmsDbService.DeleteAsync(ctx, firearm.ToFirearm());
                    if (r.IsSuccess)
                    {
                        _firearmViewModelCache.Remove(firearm);
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Failed to delete firearm {Id}.", firearm.Id);
                }
            }
        }

        [RelayCommand(CanExecute = nameof(CanEditOrDeleteFirearm))]
        async Task EditFirearmAsync(FirearmViewModel? firearm)
        {
            if (firearm is null)
            {
                return;
            }

            EditFirearmViewModel vm = new(firearm.CloneFirearmViewModel(), _firearmsDbService, _dialogServiceFactory,
                                          _sqliteHelper, _logger);
            FirearmViewModel? result = await this.ShowOverlayDialogAsync<FirearmViewModel>(
                                        "Edit firearm", vm);


            if (result is not null)
            {
                _firearmViewModelCache.AddOrUpdate(result);
            }
        }

        [RelayCommand]
        async Task RefreshAsync()
        {
            long prevId = SelectedFirearm?.Id ?? -1;
            _firearmViewModelCache.Clear();
            await LoadDataAsync();
            Optional<FirearmViewModel> prevFirearm = _firearmViewModelCache.Lookup(prevId);

            Dispatcher.UIThread.Post(() =>
                                         SelectedFirearm = prevFirearm.HasValue ? prevFirearm.Value : null);
        }

        /// <summary>
        ///     Creates a filter function for text-based filtering of SimpleRangeEvents.
        ///     Searches in both title and description fields using case-insensitive comparison.
        /// </summary>
        /// <param name="filterText">The text to search for (null/empty shows all items)</param>
        /// <returns>Filter function for use with DynamicData</returns>
        static Func<FirearmViewModel, bool> FilterByNameObservable(string? filterText) =>
            item =>
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