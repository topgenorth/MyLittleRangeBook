using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Fisher;
using Fisher.Linq;
using MyLittleRangeBook.Firearms;
using MyLittleRangeBook.GUI.Services;
using MyLittleRangeBook.Persistence;
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
        readonly             IDialogService      _dialogService;
        readonly             ILogger             _logger;
        readonly             IDocumentSession    _session;
        [ObservableProperty] IEnumerable<string> _ammoDescription = [];
        [ObservableProperty] IEnumerable<string> _firearmNames    = [];
        [ObservableProperty] IEnumerable<string> _rangeNames      = [];


        public EditSimpleRangeEventViewModel(SimpleRangeEventViewModel                simpleRangeEvent,
                                             ILogger                                  logger,
                                             Func<IDialogParticipant, IDialogService> dialogServiceFactory,
                                             IDocumentSession                         session)
        {
            Item           = simpleRangeEvent;
            _dialogService = dialogServiceFactory(this);
            _logger        = logger;
            _session       = session;


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
                if (string.IsNullOrWhiteSpace(Item.FirearmName))
                {
                    AmmoDescription = await _session.Query<SimpleRangeEvent>()
                                                    .Where(s=> !string.IsNullOrWhiteSpace(s.FirearmName))
                                                    .Where(s => s.FirearmName == Item.FirearmName)
                                                    .DistinctBy(s => s.AmmoDescription)
                                                    .OrderBy(s => s.AmmoDescription)
                                                    .Select(s => s.AmmoDescription)
                                                    .ToListAsync();
                }
                else
                {
                    AmmoDescription = await _session.Query<SimpleRangeEvent>()
                                                    .Where(s=> !string.IsNullOrWhiteSpace(s.FirearmName))
                                                    .DistinctBy(s => s.AmmoDescription)
                                                    .OrderBy(s => s.AmmoDescription)
                                                    .Select(s => s.AmmoDescription!)
                                                    .ToListAsync();
                }
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

            // await using DapperCommandContext ctx =
            //     await DapperCommandContext.NewAsync(_sqliteHelper, cancellationToken, true).ConfigureAwait(false);

            try
            {
                // Result<Guid> r1 = await _simpleRangeEventDataProcessor
                //                          .ProcessSimpleRangeEventData(ctx,
                //                                                       Item.FirearmName,
                //                                                       Item.RoundsFired,
                //                                                       Item.RangeName,
                //                                                       Item.AmmoDescription,
                //                                                       Item.Notes,
                //                                                       DateOnly.FromDateTime(Item.EventDate))
                //                          .ConfigureAwait(false);

                // if (r1.IsSuccess)
                // {
                //     await ctx.CommitAsync().ConfigureAwait(false);
                //     Result<SimpleRangeEvent> r3 = await _simpleRangeEventService
                //                                        .GetAsync(r1.Value, cancellationToken)
                //                                        .ConfigureAwait(false);
                //     SimpleRangeEvent          sre              = r3.Value;
                //     SimpleRangeEventViewModel updatedViewModel = new(sre);
                //     _dialogService.ReturnResultFromOverlayDialog(updatedViewModel);
                //     WeakReferenceMessenger.Default.Send(new UpdateDataMessage<SimpleRangeEvent>(
                //                                              Item.RowId == null
                //                                                  ? UpdateAction.Added
                //                                                  : UpdateAction.Updated,
                //                                              sre));
                // }
                // else
                // {
                //     await ctx.RollbackAsync().ConfigureAwait(false);
                //     await _dialogService.ShowOverlayDialogAsync<bool>("Error",
                //                                                       "An error occured while trying to save the event.",
                //                                                       DialogCommands.Ok);
                // }
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