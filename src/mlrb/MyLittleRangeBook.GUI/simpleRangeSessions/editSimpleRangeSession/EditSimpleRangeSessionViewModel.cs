using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Fisher;
using Fisher.Linq;
using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.RangeEvents;
using SharedControls.Controls;
using SharedControls.Services;

namespace MyLittleRangeBook.GUI
{
    /// <summary>
    ///     ViewModel responsible for editing a simple range event.
    ///     This class provides functionality for handling user inputs and interactions
    ///     related to editing range events, as well as integrating with services for logging and persistence.
    /// </summary>
    public partial class EditSimpleRangeSessionViewModel : ViewModelBase, IDialogParticipant
    {
        readonly             IDialogService      _dialogService;
        readonly             ILogger             _logger;
        readonly             IDocumentSession    _session;
        [ObservableProperty] IEnumerable<string> _ammoDescription = [];
        string?                                  _firearmNameSearchText;
        [ObservableProperty] IEnumerable<string> _firearmNames = [];
        [ObservableProperty] IEnumerable<string> _rangeNames   = [];

        public EditSimpleRangeSessionViewModel(Func<IDialogParticipant, IDialogService> dialogServiceFactory,
                                               ILogger                                  logger,
                                               IDocumentSession                         session,
                                               SimpleRangeSessionViewModel              simpleRangeSession)
        {
            Item = simpleRangeSession;

            _dialogService         = dialogServiceFactory(this);
            _logger                = logger;
            _session               = session;
            _firearmNameSearchText = simpleRangeSession.FirearmName;

            _ = LoadFirearmNamesAsync();
            _ = LoadAmmoDescriptionsAsync(simpleRangeSession.FirearmName);
            _ = LoadRangeNamesAsync();
        }

        public SimpleRangeSessionViewModel Item { get; }


        public string? FirearmNameSearchText
        {
            get => _firearmNameSearchText;
            set
            {
                if (SetProperty(ref _firearmNameSearchText, value))
                {
                    Item.FirearmName = value ?? string.Empty;
                    _                = UpdateAmmoDescriptions(value);
                }
            }
        }

        async Task UpdateAmmoDescriptions(string? searchText, CancellationToken cancellationToken = default) =>
            await LoadAmmoDescriptionsAsync(searchText?.Trim(), cancellationToken);

        async Task LoadRangeNamesAsync()
        {
            try
            {
                RangeNames = await _session.Query<SimpleRangeSession>()
                                           .DistinctBy(s => s.RangeName)
                                           .Where(s => !string.IsNullOrEmpty(s.RangeName))
                                           .OrderBy(s => s.RangeName)
                                           .Select(s => s.RangeName)
                                           .ToListAsync();
            }
            catch (Exception e)
            {
                RangeNames = ["Error loading range names"];
                _logger.Error(e, "Could not load range names.");
            }
        }

        /// <summary>
        ///     Asynchronously retrieves ammunition descriptions for a specified firearm.
        /// </summary>
        /// <param name="firearmName">
        ///     The name of the firearm for which to retrieve ammunition descriptions.
        /// </param>
        /// <param name="cancellationToken">
        ///     A token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation of retrieving ammunition descriptions.
        /// </returns>
        async Task<IReadOnlyList<string>> GetAmmoDescriptionsForFirearm(
            string? firearmName, CancellationToken cancellationToken)
        {
            const string SQL =
                "SELECT DISTINCT ammo_description FROM firearm_ammo_descriptions ORDER BY ammo_description;";
            const string SQL_FIREARM =
                "SELECT DISTINCT ammo_description FROM firearm_ammo_descriptions WHERE firearm_name = ? ORDER BY ammo_description;";

            IReadOnlyList<string> descriptions;
            if (string.IsNullOrWhiteSpace(firearmName))
            {
                descriptions = await _session.AdvancedSql.QueryAsync<string>(SQL, cancellationToken);
            }
            else
            {
                object[] p = [firearmName];
                descriptions = await _session.AdvancedSql.QueryAsync<string>(SQL_FIREARM, cancellationToken, p);
            }

            return descriptions;
        }

        /// <summary>
        ///     Asynchronously loads ammunition descriptions based on the optionally provided firearm name.
        ///     If no firearm name is specified, retrieves a general list of ammunition descriptions.
        ///     Handles any errors that occur during the loading process and logs appropriate error messages.
        /// </summary>
        /// <param name="firearmName">
        ///     The name of the firearm for which ammunition descriptions should be loaded.
        ///     If null, ammunition descriptions for all firearms are loaded.
        /// </param>
        /// <param name="cancellationToken">
        ///     A token to monitor for cancellation requests.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation of loading ammunition descriptions.
        /// </returns>
        async Task LoadAmmoDescriptionsAsync(string? firearmName = null, CancellationToken cancellationToken = default)
        {
            try
            {
                IReadOnlyList<string> descriptions =
                    await GetAmmoDescriptionsForFirearm(firearmName, cancellationToken);
                AmmoDescription = descriptions;
                _logger.Verbose("Loaded {Count} ammo descriptions for {FirearmName}",
                                descriptions.Count,
                                firearmName ?? "NULL");
            }
            catch (Exception e)
            {
                AmmoDescription = ["Error loading ammo descriptions"];
                _logger.Error(e, "Could not load ammo descriptions");
            }
        }

        async Task LoadFirearmNamesAsync()
        {
            try
            {
                FirearmNames = await _session.Query<Firearm>()
                                             .Where(f => f.IsActive)
                                             .OrderBy(f => f.Name)
                                             .Select(f => f.Name)
                                             .ToListAsync();
            }
            catch (Exception e)
            {
                FirearmNames = ["Error loading firearms"];
                _logger.Error(e, "Could not load firearm names");
            }
        }

        [RelayCommand]
        async Task SaveAsync(CancellationToken cancellationToken = default)
        {
            Item.Validate();
            if (Item.HasErrors)
            {
                await _dialogService.ShowOverlayDialogAsync<DialogResult>("Validation Error",
                                                                          "Please correct the errors in the form before saving.",
                                                                          DialogCommands.Ok);
                return;
            }


            try
            {
                List<object> events = Item.GetEvents();
                if (events.Count > 0)
                {
                    _session.Events.Append(Item.Id, events);
                    await _session.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    _logger.Verbose("Nothing was done with the simple range session {Id}", Item.Id);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to save simple range event {Id}", Item.Id);
                await _dialogService.ShowOverlayDialogAsync<bool>("Error",
                                                                  "An error occured while trying to save the event.",
                                                                  DialogCommands.Ok);
            }
        }

        [RelayCommand]
        async Task CancelAsync()
        {
            DialogCommand[] commands = [DialogCommands.No, DialogCommands.Yes];
            DialogResult userResponse = await _dialogService.ShowOverlayDialogAsync<DialogResult>(
                                             "Cancel editing?",
                                             "Do you want to discard your changes?",
                                             commands);

            switch (userResponse)
            {
                case DialogResult.Yes:
                    _dialogService.ReturnResultFromOverlayDialog(null);

                    break;
                case DialogResult.No:
                    break;
                case DialogResult.None:
                case DialogResult.Ok:
                case DialogResult.Cancel:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}