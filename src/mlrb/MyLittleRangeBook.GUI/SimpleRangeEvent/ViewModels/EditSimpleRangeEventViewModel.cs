using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Fisher;
using Fisher.Linq;
using FluentResults;
using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.GUI.Messages;
using MyLittleRangeBook.GUI.Services;
using MyLittleRangeBook.RangeEvents;
using SharedControls.Controls;
using SharedControls.Services;

namespace MyLittleRangeBook.GUI.ViewModels
{
    /// <summary>
    ///     ViewModel responsible for editing a simple range event.
    ///     This class provides functionality for handling user inputs and interactions
    ///     related to editing range events, as well as integrating with services for logging and persistence.
    /// </summary>
    public partial class EditSimpleRangeEventViewModel : ViewModelBase, IDialogParticipant
    {
        readonly             IDialogService           _dialogService;
        readonly             ILogger                  _logger;
        readonly             ISimpleRangeEventService _rangeEventService;
        readonly             IDocumentSession         _session;
        [ObservableProperty] IEnumerable<string>      _ammoDescription = [];
        [ObservableProperty] IEnumerable<string>      _firearmNames    = [];
        [ObservableProperty] IEnumerable<string>      _rangeNames      = [];


        public EditSimpleRangeEventViewModel(Func<IDialogParticipant, IDialogService> dialogServiceFactory,
                                             ILogger                                  logger,
                                             IDocumentSession                         session,
                                             SimpleRangeEventViewModel                simpleRangeEvent,
                                             ISimpleRangeEventService                 rangeEventService)
        {
            Item               = simpleRangeEvent;
            _rangeEventService = rangeEventService;
            _dialogService     = dialogServiceFactory(this);
            _logger            = logger;
            _session           = session;

            _ = LoadFirearmNamesAsync();
            _ = LoadAmmoDescriptionsAsync();
            _ = LoadRangeNamesAsync();
        }

        public SimpleRangeEventViewModel Item { get; }

        async Task LoadRangeNamesAsync() =>
            RangeNames = await _session.Query<SimpleRangeEvent>()
                                       .DistinctBy(s => s.RangeName)
                                       .Where(s => !string.IsNullOrEmpty(s.RangeName))
                                       .OrderBy(s => s.RangeName)
                                       .Select(s => s.RangeName)
                                       .ToListAsync();

        async Task LoadAmmoDescriptionsAsync()
        {
            try
            {
                // TODO [TO20260921] Limit this to the firearm name where possible.
                AmmoDescription = await _session.Query<SimpleRangeEvent>()
                                                .Where(s => !string.IsNullOrWhiteSpace(s.FirearmName))
                                                .Where(s => s.FirearmName == Item.FirearmName)
                                                .DistinctBy(s => s.AmmoDescription)
                                                .OrderBy(s => s.AmmoDescription)
                                                .Select(s => s.AmmoDescription)
                                                .ToListAsync();
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
                _logger.Error(e, "Could not load firearm names.");
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
                Result<Guid> r1 =
                    await _rangeEventService.UpsertAsync(Item.ToSimpleRangeEvent(), Item.IsNew, cancellationToken);

                if (r1.IsSuccess)
                {
                    SimpleRangeEvent? sre = await _session.Query<SimpleRangeEvent>()
                                                          .Where(e => e.Id == r1.Value)
                                                          .FirstOrDefaultAsync(cancellationToken);
                    SimpleRangeEventViewModel updatedViewModel = new(sre!);
                    _dialogService.ReturnResultFromOverlayDialog(updatedViewModel);
                    WeakReferenceMessenger.Default.Send(new UpdateDataMessage<SimpleRangeEvent>(
                                                             Item.IsNew
                                                                 ? UpdateAction.Added
                                                                 : UpdateAction.Updated,
                                                             sre));
                }
                else
                {
                    await _dialogService.ShowOverlayDialogAsync<bool>("Error",
                                                                      "An error occured while trying to save the event.",
                                                                      DialogCommands.Ok);
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